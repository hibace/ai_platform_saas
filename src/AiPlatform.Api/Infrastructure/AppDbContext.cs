using AiPlatform.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AiPlatform.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AgentEntity> Agents => Set<AgentEntity>();
    public DbSet<PolicyEntity> Policies => Set<PolicyEntity>();
    public DbSet<ToolEntity> Tools => Set<ToolEntity>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<AgentEntity>(e =>
        {
            e.ToTable("agents");
            e.HasKey(x => x.Id);
            e.Property(x => x.ToolIds).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });
        mb.Entity<PolicyEntity>(e =>
        {
            e.ToTable("policies");
            e.HasKey(x => x.Id);
            e.OwnsMany(x => x.Rules, r => r.ToJson());
        });
        mb.Entity<ToolEntity>(e =>
        {
            e.ToTable("tools");
            e.HasKey(x => x.Id);
        });
    }
}

public class AgentEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public List<string> ToolIds { get; set; } = new();
    public string? RagCollectionId { get; set; }
    public string? PolicyId { get; set; }
    public string? LlmProvider { get; set; }
    public string? LlmModel { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? TenantId { get; set; }
}

public class PolicyEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PolicyRuleEntity> Rules { get; set; } = new();
    public string? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PolicyRuleEntity
{
    public string Action { get; set; } = string.Empty;
    public string? Resource { get; set; }
    public string? ToolPattern { get; set; }
    public Dictionary<string, string>? Conditions { get; set; }
}

public class ToolEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? InputSchema { get; set; }
    public string? TenantId { get; set; }
    public bool IsEnabled { get; set; } = true;
}
