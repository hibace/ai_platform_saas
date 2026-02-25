using AiPlatform.Core.Domain;

namespace AiPlatform.Agents;

public interface IAgentRuntime
{
    Task<AgentResponse> RunAsync(AgentRunRequest request, CancellationToken ct = default);
}

public record AgentRunRequest(
    Guid AgentId,
    string? TenantId,
    string? UserId,
    string Message,
    IReadOnlyDictionary<string, string>? Context = null);

public record AgentResponse(
    string Message,
    IReadOnlyList<ToolCallRecord> ToolCalls,
    bool Completed,
    string? Error = null);

public record ToolCallRecord(string ToolId, string Input, string? Output, bool Success);
