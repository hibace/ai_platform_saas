using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AiPlatform.Api.Infrastructure;

public class EfPolicyRepository : IPolicyRepository
{
    private readonly AppDbContext _db;

    public EfPolicyRepository(AppDbContext db) => _db = db;

    public async Task<Policy?> GetByIdAsync(Guid id, string? tenantId, CancellationToken ct = default)
    {
        var q = _db.Policies.AsNoTracking().Where(p => p.Id == id);
        if (!string.IsNullOrEmpty(tenantId)) q = q.Where(p => p.TenantId == tenantId);
        var e = await q.Include(p => p.Rules).FirstOrDefaultAsync(ct).ConfigureAwait(false);
        return e == null ? null : ToPolicy(e);
    }

    public async Task<IReadOnlyList<Policy>> ListAsync(string? tenantId, CancellationToken ct = default)
    {
        IQueryable<PolicyEntity> q = _db.Policies.AsNoTracking().Include(p => p.Rules).OrderBy(p => p.Name);
        if (!string.IsNullOrEmpty(tenantId)) q = q.Where(p => p.TenantId == tenantId);
        var list = await q.ToListAsync(ct).ConfigureAwait(false);
        return list.Select(ToPolicy).ToList();
    }

    public async Task<Policy> SaveAsync(Policy policy, CancellationToken ct = default)
    {
        var e = await _db.Policies.Include(p => p.Rules).FirstOrDefaultAsync(p => p.Id == policy.Id, ct).ConfigureAwait(false);
        if (e == null)
        {
            e = new PolicyEntity
            {
                Id = policy.Id == default ? Guid.NewGuid() : policy.Id,
                Name = policy.Name,
                Description = policy.Description,
                TenantId = policy.TenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Rules = policy.Rules.Select(r => new PolicyRuleEntity { Action = r.Action, Resource = r.Resource, ToolPattern = r.ToolPattern, Conditions = r.Conditions }).ToList()
            };
            _db.Policies.Add(e);
        }
        else
        {
            e.Name = policy.Name;
            e.Description = policy.Description;
            e.UpdatedAt = DateTime.UtcNow;
            e.Rules = policy.Rules.Select(r => new PolicyRuleEntity { Action = r.Action, Resource = r.Resource, ToolPattern = r.ToolPattern, Conditions = r.Conditions }).ToList();
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return ToPolicy(e);
    }

    private static Policy ToPolicy(PolicyEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        TenantId = e.TenantId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Rules = e.Rules.Select(r => new PolicyRule { Action = r.Action, Resource = r.Resource, ToolPattern = r.ToolPattern, Conditions = r.Conditions }).ToArray()
    };
}
