using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AiPlatform.Api.Infrastructure;

public class EfToolRegistry : IToolRegistry
{
    private readonly AppDbContext _db;

    public EfToolRegistry(AppDbContext db) => _db = db;

    public async Task<ToolDefinition?> GetByIdAsync(string id, string? tenantId, CancellationToken ct = default)
    {
        var q = _db.Tools.AsNoTracking().Where(t => t.Id == id);
        if (!string.IsNullOrEmpty(tenantId)) q = q.Where(t => t.TenantId == tenantId || t.TenantId == null);
        var e = await q.FirstOrDefaultAsync(ct).ConfigureAwait(false);
        return e == null ? null : ToDef(e);
    }

    public async Task<IReadOnlyList<ToolDefinition>> ListAsync(string? tenantId, CancellationToken ct = default)
    {
        var q = _db.Tools.AsNoTracking().Where(t => t.TenantId == tenantId || t.TenantId == null).OrderBy(t => t.Name);
        var list = await q.ToListAsync(ct).ConfigureAwait(false);
        return list.Select(ToDef).ToList();
    }

    public async Task<ToolDefinition> RegisterAsync(ToolDefinition tool, CancellationToken ct = default)
    {
        var e = await _db.Tools.FirstOrDefaultAsync(t => t.Id == tool.Id, ct).ConfigureAwait(false);
        if (e == null)
        {
            e = new ToolEntity
            {
                Id = tool.Id,
                Name = tool.Name,
                Description = tool.Description,
                InputSchema = tool.InputSchema,
                TenantId = tool.TenantId,
                IsEnabled = tool.IsEnabled
            };
            _db.Tools.Add(e);
        }
        else
        {
            e.Name = tool.Name;
            e.Description = tool.Description;
            e.InputSchema = tool.InputSchema;
            e.IsEnabled = tool.IsEnabled;
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return ToDef(e);
    }

    private static ToolDefinition ToDef(ToolEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        InputSchema = e.InputSchema,
        TenantId = e.TenantId,
        IsEnabled = e.IsEnabled
    };
}
