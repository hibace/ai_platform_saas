namespace AiPlatform.Agents;

/// <summary>
/// Abstraction for external LLM (OpenAI-compatible, Azure OpenAI, etc.).
/// BYOC: customer brings own endpoint and key.
/// </summary>
public interface ILlmClient
{
    string ProviderId { get; }
    Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken ct = default);
}

public record LlmRequest(
    string SystemPrompt,
    IReadOnlyList<ChatMessage> Messages,
    IReadOnlyList<LlmToolSpec>? Tools = null,
    double Temperature = 0.7);

public record ChatMessage(string Role, string Content);

public record LlmToolSpec(string Name, string Description, string? ParametersJson);

public record LlmResponse(string Content, IReadOnlyList<LlmToolCall>? ToolCalls = null);

public record LlmToolCall(string Name, string ArgumentsJson);
