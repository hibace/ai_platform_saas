namespace AiPlatform.Core.Domain;

public class Policy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PolicyRule[] Rules { get; set; } = Array.Empty<PolicyRule>();
    public string? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PolicyRule
{
    public string Action { get; set; } = string.Empty; // allow, deny
    public string? Resource { get; set; }
    public string? ToolPattern { get; set; }
    public Dictionary<string, string>? Conditions { get; set; }
}
