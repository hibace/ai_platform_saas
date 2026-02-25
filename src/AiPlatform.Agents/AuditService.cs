using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;

namespace AiPlatform.Agents;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public Task LogToolInvocationAsync(Guid? agentId, string toolName, string? input, string? output, string? tenantId, string correlationId, CancellationToken ct = default)
    {
        return _auditRepository.AppendAsync(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EventType = "tool_invocation",
            AgentId = agentId?.ToString(),
            TenantId = tenantId,
            ToolName = toolName,
            Payload = input,
            Result = output,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        }, ct);
    }

    public Task LogToolBlockedAsync(Guid? agentId, string toolName, string? tenantId, string correlationId, CancellationToken ct = default)
    {
        return _auditRepository.AppendAsync(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EventType = "tool_blocked",
            AgentId = agentId?.ToString(),
            TenantId = tenantId,
            ToolName = toolName,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        }, ct);
    }

    public Task LogAgentCompletionAsync(Guid? agentId, string? tenantId, string? userId, string correlationId, CancellationToken ct = default)
    {
        return _auditRepository.AppendAsync(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EventType = "agent_completion",
            AgentId = agentId?.ToString(),
            TenantId = tenantId,
            UserId = userId,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        }, ct);
    }
}
