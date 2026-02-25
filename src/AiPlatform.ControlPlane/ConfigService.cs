using Microsoft.Extensions.Configuration;

namespace AiPlatform.ControlPlane;

public class ConfigService : IConfigService
{
    private readonly IConfiguration _configuration;

    public ConfigService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? Get(string key) => _configuration[key];

    public T? GetSection<T>(string section) where T : class, new()
    {
        var s = _configuration.GetSection(section).Get<T>();
        return s;
    }

    public Task SetAsync(string key, string value, CancellationToken ct = default)
    {
        // In self-hosted, config is typically file/env; override via config file or external store.
        return Task.CompletedTask;
    }
}
