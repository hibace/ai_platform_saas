using AiPlatform.Agents;
using AiPlatform.Api.Models;
using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AiPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/agents")]
public class AgentsController : ControllerBase
{
    private readonly IAgentRepository _agents;
    private readonly IAgentRuntime _runtime;

    public AgentsController(IAgentRepository agents, IAgentRuntime runtime)
    {
        _agents = agents;
        _runtime = runtime;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AgentDefinition>>> List([FromQuery] string? tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        var list = await _agents.ListAsync(tenantId, skip, take, ct).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AgentDefinition>> Get(Guid id, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var agent = await _agents.GetByIdAsync(id, tenantId, ct).ConfigureAwait(false);
        if (agent == null) return NotFound();
        return Ok(agent);
    }

    [HttpPost]
    public async Task<ActionResult<AgentDefinition>> Create([FromBody] CreateAgentRequest req, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var agent = new AgentDefinition
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description ?? "",
            SystemPrompt = req.SystemPrompt ?? "",
            ToolIds = req.ToolIds ?? Array.Empty<string>(),
            RagCollectionId = req.RagCollectionId,
            PolicyId = req.PolicyId,
            LlmProvider = req.LlmProvider,
            LlmModel = req.LlmModel,
            IsEnabled = req.IsEnabled ?? true,
            TenantId = tenantId
        };
        agent = await _agents.CreateAsync(agent, ct).ConfigureAwait(false);
        return CreatedAtAction(nameof(Get), new { id = agent.Id }, agent);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AgentDefinition>> Update(Guid id, [FromBody] UpdateAgentRequest req, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var existing = await _agents.GetByIdAsync(id, tenantId, ct).ConfigureAwait(false);
        if (existing == null) return NotFound();
        var agent = new AgentDefinition
        {
            Id = id,
            Name = req.Name ?? existing.Name,
            Description = req.Description ?? existing.Description,
            SystemPrompt = req.SystemPrompt ?? existing.SystemPrompt,
            ToolIds = req.ToolIds ?? existing.ToolIds,
            RagCollectionId = req.RagCollectionId ?? existing.RagCollectionId,
            PolicyId = req.PolicyId ?? existing.PolicyId,
            LlmProvider = req.LlmProvider ?? existing.LlmProvider,
            LlmModel = req.LlmModel ?? existing.LlmModel,
            IsEnabled = req.IsEnabled ?? existing.IsEnabled,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            TenantId = tenantId ?? existing.TenantId
        };
        agent = await _agents.UpdateAsync(agent, ct).ConfigureAwait(false);
        return Ok(agent);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        await _agents.DeleteAsync(id, tenantId, ct).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{id:guid}/run")]
    public async Task<ActionResult<ApiResult<AgentResponse>>> Run(Guid id, [FromBody] RunAgentRequest req, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _runtime.RunAsync(new AgentRunRequest(id, tenantId, req.UserId, req.Message, req.Context), ct).ConfigureAwait(false);
            return Ok(ApiResult<AgentResponse>.Success(response));
        }
        catch (Exception ex)
        {
            return Ok(ApiResult<AgentResponse>.Failure(ex.Message));
        }
    }
}

public record CreateAgentRequest(string Name, string? Description, string? SystemPrompt, IReadOnlyList<string>? ToolIds, string? RagCollectionId, string? PolicyId, string? LlmProvider, string? LlmModel, bool? IsEnabled);
public record UpdateAgentRequest(string? Name, string? Description, string? SystemPrompt, IReadOnlyList<string>? ToolIds, string? RagCollectionId, string? PolicyId, string? LlmProvider, string? LlmModel, bool? IsEnabled);
public record RunAgentRequest(string Message, string? UserId, IReadOnlyDictionary<string, string>? Context);
