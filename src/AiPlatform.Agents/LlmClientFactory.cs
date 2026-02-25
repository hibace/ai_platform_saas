using AiPlatform.Agents.Llm;
using Microsoft.Extensions.Options;

namespace AiPlatform.Agents;

public class LlmClientFactory : ILlmClientFactory
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly LlmOptions _options;

    public LlmClientFactory(IHttpClientFactory httpFactory, IOptions<LlmOptions> options)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
    }

    public ILlmClient? GetClient(string? provider, string? model)
    {
        var baseUrl = _options.BaseUrl ?? provider;
        var m = model ?? _options.DefaultModel ?? "gpt-4o-mini";
        if (string.IsNullOrEmpty(baseUrl)) return null;
        var client = _httpFactory.CreateClient("llm");
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        if (!string.IsNullOrEmpty(_options.ApiKey))
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _options.ApiKey);
        return new OpenAiCompatibleClient(client, m);
    }
}

public class LlmOptions
{
    public const string SectionName = "Llm";
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? DefaultModel { get; set; }
}
