using AiPlatform.Core.Domain;

namespace AiPlatform.Core.Repositories;

public interface IToolRegistry
{
    Task<ToolDefinition?> GetByIdAsync(string id, string? tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ToolDefinition>> ListAsync(string? tenantId, CancellationToken ct = default);
    Task<ToolDefinition> RegisterAsync(ToolDefinition tool, CancellationToken ct = default);
}
