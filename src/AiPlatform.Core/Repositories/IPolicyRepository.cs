using AiPlatform.Core.Domain;

namespace AiPlatform.Core.Repositories;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(Guid id, string? tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Policy>> ListAsync(string? tenantId, CancellationToken ct = default);
    Task<Policy> SaveAsync(Policy policy, CancellationToken ct = default);
}
