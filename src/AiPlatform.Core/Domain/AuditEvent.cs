namespace AiPlatform.Core.Domain;

public class AuditEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? AgentId { get; set; }
    public string? TenantId { get; set; }
    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ToolName { get; set; }
    public string? Payload { get; set; }
    public string? Result { get; set; }
    public int? StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
