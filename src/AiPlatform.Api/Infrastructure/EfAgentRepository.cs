using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AiPlatform.Api.Infrastructure;

public class EfAgentRepository : IAgentRepository
{
    private readonly AppDbContext _db;

    public EfAgentRepository(AppDbContext db) => _db = db;

    public async Task<AgentDefinition?> GetByIdAsync(Guid id, string? tenantId, CancellationToken ct = default)
    {
        var q = _db.Agents.AsNoTracking().Where(a => a.Id == id);
        if (!string.IsNullOrEmpty(tenantId)) q = q.Where(a => a.TenantId == tenantId);
        var e = await q.FirstOrDefaultAsync(ct).ConfigureAwait(false);
        return e == null ? null : ToDef(e);
    }

    public async Task<IReadOnlyList<AgentDefinition>> ListAsync(string? tenantId, int skip, int take, CancellationToken ct = default)
    {
        IQueryable<AgentEntity> q = _db.Agents.AsNoTracking().OrderBy(a => a.Name);
        if (!string.IsNullOrEmpty(tenantId)) q = q.Where(a => a.TenantId == tenantId);
        var list = await q.Skip(skip).Take(take).ToListAsync(ct).ConfigureAwait(false);
        return list.Select(ToDef).ToList();
    }

    public async Task<AgentDefinition> CreateAsync(AgentDefinition agent, CancellationToken ct = default)
    {
        var e = new AgentEntity
        {
            Id = agent.Id == default ? Guid.NewGuid() : agent.Id,
            Name = agent.Name,
            Description = agent.Description,
            SystemPrompt = agent.SystemPrompt,
            ToolIds = agent.ToolIds.ToList(),
            RagCollectionId = agent.RagCollectionId,
            PolicyId = agent.PolicyId,
            LlmProvider = agent.LlmProvider,
            LlmModel = agent.LlmModel,
            IsEnabled = agent.IsEnabled,
            TenantId = agent.TenantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Agents.Add(e);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return ToDef(e);
    }

    public async Task<AgentDefinition> UpdateAsync(AgentDefinition agent, CancellationToken ct = default)
    {
        var e = await _db.Agents.FirstOrDefaultAsync(a => a.Id == agent.Id, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Agent not found.");
        e.Name = agent.Name;
        e.Description = agent.Description;
        e.SystemPrompt = agent.SystemPrompt;
        e.ToolIds = agent.ToolIds.ToList();
        e.RagCollectionId = agent.RagCollectionId;
        e.PolicyId = agent.PolicyId;
        e.LlmProvider = agent.LlmProvider;
        e.LlmModel = agent.LlmModel;
        e.IsEnabled = agent.IsEnabled;
        e.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return ToDef(e);
    }

    public async Task DeleteAsync(Guid id, string? tenantId, CancellationToken ct = default)
    {
        var q = _db.Agents.Where(a => a.Id == id);
        if (!string.IsNullOrEmpty(tenantId)) q = q.Where(a => a.TenantId == tenantId);
        var e = await q.FirstOrDefaultAsync(ct).ConfigureAwait(false);
        if (e != null) { _db.Agents.Remove(e); await _db.SaveChangesAsync(ct).ConfigureAwait(false); }
    }

    private static AgentDefinition ToDef(AgentEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        SystemPrompt = e.SystemPrompt,
        ToolIds = e.ToolIds,
        RagCollectionId = e.RagCollectionId,
        PolicyId = e.PolicyId,
        LlmProvider = e.LlmProvider,
        LlmModel = e.LlmModel,
        IsEnabled = e.IsEnabled,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        TenantId = e.TenantId
    };
}
