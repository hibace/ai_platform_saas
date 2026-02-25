namespace AiPlatform.Agents;

public interface IAuditService
{
    Task LogToolInvocationAsync(Guid? agentId, string toolName, string? input, string? output, string? tenantId, string correlationId, CancellationToken ct = default);
    Task LogToolBlockedAsync(Guid? agentId, string toolName, string? tenantId, string correlationId, CancellationToken ct = default);
    Task LogAgentCompletionAsync(Guid? agentId, string? tenantId, string? userId, string correlationId, CancellationToken ct = default);
}
