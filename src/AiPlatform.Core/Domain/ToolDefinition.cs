namespace AiPlatform.Core.Domain;

public class ToolDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? InputSchema { get; set; }
    public string? TenantId { get; set; }
    public bool IsEnabled { get; set; } = true;
}
