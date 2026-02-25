using System.Text.Json;

namespace AiPlatform.Agents.Tools;

public class EchoTool : IToolExecutor
{
    public string ToolId => "echo";

    public Task<ToolResult> ExecuteAsync(ToolInvocation invocation, CancellationToken ct = default)
    {
        try
        {
            var json = string.IsNullOrEmpty(invocation.ArgumentsJson) ? "{}" : invocation.ArgumentsJson;
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            var msg = doc.TryGetProperty("message", out var m) ? m.GetString() ?? invocation.ArgumentsJson : invocation.ArgumentsJson;
            return Task.FromResult(new ToolResult(true, $"Echo: {msg}", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ToolResult(false, null, ex.Message));
        }
    }
}
