using System.Text.Json;
using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace AiPlatform.Agents;

public class AgentRuntime : IAgentRuntime
{
    private readonly IAgentRepository _agentRepository;
    private readonly IToolRegistry _toolRegistry;
    private readonly ILlmClientFactory _llmFactory;
    private readonly IToolExecutorFactory _toolFactory;
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IAuditService _auditService;
    private readonly ILogger<AgentRuntime> _logger;
    private const int MaxToolIterations = 10;

    public AgentRuntime(
        IAgentRepository agentRepository,
        IToolRegistry toolRegistry,
        ILlmClientFactory llmFactory,
        IToolExecutorFactory toolFactory,
        IPolicyEvaluator policyEvaluator,
        IAuditService auditService,
        ILogger<AgentRuntime> logger)
    {
        _agentRepository = agentRepository;
        _toolRegistry = toolRegistry;
        _llmFactory = llmFactory;
        _toolFactory = toolFactory;
        _policyEvaluator = policyEvaluator;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<AgentResponse> RunAsync(AgentRunRequest request, CancellationToken ct = default)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var agent = await _agentRepository.GetByIdAsync(request.AgentId, request.TenantId, ct).ConfigureAwait(false);
        if (agent == null)
            return new AgentResponse("", Array.Empty<ToolCallRecord>(), false, "Agent not found.");

        if (!agent.IsEnabled)
            return new AgentResponse("", Array.Empty<ToolCallRecord>(), false, "Agent is disabled.");

        var llm = _llmFactory.GetClient(agent.LlmProvider, agent.LlmModel);
        if (llm == null)
            return new AgentResponse("", Array.Empty<ToolCallRecord>(), false, "LLM not configured.");

        var tools = await ResolveToolsAsync(agent.ToolIds, request.TenantId, ct).ConfigureAwait(false);
        var toolCalls = new List<ToolCallRecord>();
        var messages = new List<ChatMessage> { new("user", request.Message) };

        for (var i = 0; i < MaxToolIterations; i++)
        {
            // OpenAI requires function.name to match ^[a-zA-Z0-9_-]+$ — we pass Id (echo, http_get), not human-readable Name (HTTP GET)
            var llmTools = tools.Select(t => new LlmToolSpec(t.Id, t.Description, t.InputSchema)).ToList();
            var llmRequest = new LlmRequest(agent.SystemPrompt, messages, llmTools);
            var response = await llm.ChatAsync(llmRequest, ct).ConfigureAwait(false);

            if (response.ToolCalls == null || response.ToolCalls.Count == 0)
            {
                await _auditService.LogAgentCompletionAsync(request.AgentId, request.TenantId, request.UserId, correlationId, ct).ConfigureAwait(false);
                return new AgentResponse(response.Content, toolCalls, true);
            }

            foreach (var call in response.ToolCalls)
            {
                var canInvoke = await _policyEvaluator.CanInvokeToolAsync(call.Name, request.TenantId, ct).ConfigureAwait(false);
                if (!canInvoke)
                {
                    toolCalls.Add(new ToolCallRecord(call.Name, call.ArgumentsJson, "Blocked by policy", false));
                    await _auditService.LogToolBlockedAsync(request.AgentId, call.Name, request.TenantId, correlationId, ct).ConfigureAwait(false);
                    continue;
                }

                var executor = _toolFactory.GetExecutor(call.Name);
                if (executor == null)
                {
                    toolCalls.Add(new ToolCallRecord(call.Name, call.ArgumentsJson, "Tool not found", false));
                    continue;
                }

                var invocation = new ToolInvocation(call.Name, call.ArgumentsJson, request.TenantId, correlationId);
                var result = await executor.ExecuteAsync(invocation, ct).ConfigureAwait(false);
                toolCalls.Add(new ToolCallRecord(call.Name, call.ArgumentsJson, result.Output ?? result.Error, result.Success));
                await _auditService.LogToolInvocationAsync(request.AgentId, call.Name, call.ArgumentsJson, result.Output, request.TenantId, correlationId, ct).ConfigureAwait(false);

                messages.Add(new ChatMessage("assistant", $"[Tool call: {call.Name}]"));
                messages.Add(new ChatMessage("user", $"Tool result: {(result.Success ? result.Output : result.Error)}"));
            }
        }

        await _auditService.LogAgentCompletionAsync(request.AgentId, request.TenantId, request.UserId, correlationId, ct).ConfigureAwait(false);
        return new AgentResponse("Max tool iterations reached.", toolCalls, true);
    }

    private async Task<IReadOnlyList<ToolDefinition>> ResolveToolsAsync(IReadOnlyList<string> toolIds, string? tenantId, CancellationToken ct)
    {
        var list = new List<ToolDefinition>();
        foreach (var id in toolIds)
        {
            var t = await _toolRegistry.GetByIdAsync(id, tenantId, ct).ConfigureAwait(false);
            if (t != null && t.IsEnabled)
                list.Add(t);
        }
        return list;
    }
}
