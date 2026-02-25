using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AiPlatform.Agents.Llm;

/// <summary>
/// HTTP client for OpenAI-compatible APIs (OpenAI, Azure OpenAI, local models).
/// </summary>
public class OpenAiCompatibleClient : ILlmClient
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly JsonSerializerOptions _jsonOptions;

    public string ProviderId => "openai-compatible";

    public OpenAiCompatibleClient(HttpClient http, string model)
    {
        _http = http;
        _model = model;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
    }

    private const int MaxRetriesOn429 = 2;
    private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(15);

    public async Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken ct = default)
    {
        var payloadJson = BuildPayloadJson(request);
        Exception? lastException = null;

        for (var attempt = 0; attempt <= MaxRetriesOn429; attempt++)
        {
            if (attempt > 0)
                await Task.Delay(DefaultRetryDelay, ct).ConfigureAwait(false);

            using var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/v1/chat/completions", content, ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return await ParseResponseAsync(response, ct).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < MaxRetriesOn429)
                continue;

            lastException = new HttpRequestException(
                response.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                    ? "LLM rate limit exceeded (429). Try again in a minute or check quota: https://platform.openai.com/usage"
                    : $"LLM error ({(int)response.StatusCode}): {body}");
            throw lastException;
        }

        throw lastException!;
    }

    private string BuildPayloadJson(LlmRequest request)
    {
        var messagesList = new List<object> { new { role = "system", content = request.SystemPrompt } };
        foreach (var m in request.Messages)
            messagesList.Add(new { role = m.Role, content = m.Content });
        var payload = new
        {
            model = _model,
            messages = messagesList,
            tools = request.Tools?.Select(t => new
            {
                type = "function",
                function = new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = t.ParametersJson != null ? JsonSerializer.Deserialize<JsonElement>(t.ParametersJson) : (object?)new { type = "object", properties = new object() }
                }
            }),
            temperature = request.Temperature
        };
        return JsonSerializer.Serialize(payload, _jsonOptions);
    }

    private async Task<LlmResponse> ParseResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions, ct).ConfigureAwait(false);
        var choice = doc.GetProperty("choices")[0];
        var message = choice.GetProperty("message");
        var text = message.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";
        var toolCalls = new List<LlmToolCall>();
        if (message.TryGetProperty("tool_calls", out var tc))
        {
            foreach (var call in tc.EnumerateArray())
            {
                var fn = call.GetProperty("function");
                toolCalls.Add(new LlmToolCall(
                    fn.GetProperty("name").GetString() ?? "",
                    fn.GetProperty("arguments").GetString() ?? "{}"));
            }
        }

        return new LlmResponse(text, toolCalls.Count > 0 ? toolCalls : null);
    }
}
