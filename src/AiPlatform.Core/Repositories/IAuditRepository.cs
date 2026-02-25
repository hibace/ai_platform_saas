using AiPlatform.Core.Domain;

namespace AiPlatform.Core.Repositories;

public interface IAuditRepository
{
    Task AppendAsync(AuditEvent evt, CancellationToken ct = default);
    Task<IReadOnlyList<AuditEvent>> QueryAsync(AuditQuery query, CancellationToken ct = default);
}

public record AuditQuery(
    string? TenantId = null,
    string? AgentId = null,
    string? EventType = null,
    DateTime? From = null,
    DateTime? To = null,
    int Skip = 0,
    int Take = 50);
