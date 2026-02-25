using System.Text.Json;

namespace AiPlatform.Agents.Tools;

public class HttpGetTool : IToolExecutor
{
    private readonly IHttpClientFactory _httpFactory;

    public string ToolId => "http_get";

    public HttpGetTool(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    public async Task<ToolResult> ExecuteAsync(ToolInvocation invocation, CancellationToken ct = default)
    {
        try
        {
            var args = JsonSerializer.Deserialize<JsonElement>(invocation.ArgumentsJson ?? "{}");
            var url = args.TryGetProperty("url", out var u) ? u.GetString() : null;
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                return new ToolResult(false, null, "Invalid or missing url.");
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var response = await client.GetAsync(uri, ct).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return new ToolResult(true, body.Length > 2000 ? body[..2000] + "..." : body, null);
        }
        catch (Exception ex)
        {
            return new ToolResult(false, null, ex.Message);
        }
    }
}
