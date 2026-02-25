namespace AiPlatform.Agents;

public interface IToolExecutor
{
    string ToolId { get; }
    Task<ToolResult> ExecuteAsync(ToolInvocation invocation, CancellationToken ct = default);
}

public record ToolInvocation(string ToolId, string ArgumentsJson, string? TenantId, string? CorrelationId);

public record ToolResult(bool Success, string? Output, string? Error);
