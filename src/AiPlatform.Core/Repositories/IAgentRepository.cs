using AiPlatform.Core.Domain;

namespace AiPlatform.Core.Repositories;

public interface IAgentRepository
{
    Task<AgentDefinition?> GetByIdAsync(Guid id, string? tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<AgentDefinition>> ListAsync(string? tenantId, int skip, int take, CancellationToken ct = default);
    Task<AgentDefinition> CreateAsync(AgentDefinition agent, CancellationToken ct = default);
    Task<AgentDefinition> UpdateAsync(AgentDefinition agent, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string? tenantId, CancellationToken ct = default);
}
