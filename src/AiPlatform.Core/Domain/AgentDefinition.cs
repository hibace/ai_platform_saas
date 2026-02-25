namespace AiPlatform.Core.Domain;

public class AgentDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public IReadOnlyList<string> ToolIds { get; set; } = Array.Empty<string>();
    public string? RagCollectionId { get; set; }
    public string? PolicyId { get; set; }
    public string? LlmProvider { get; set; }
    public string? LlmModel { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? TenantId { get; set; }
}
