namespace AiPlatform.ControlPlane;

/// <summary>
/// SaaS control plane: runtime config (BYOC LLM endpoint, feature flags, tenant overrides).
/// </summary>
public interface IConfigService
{
    string? Get(string key);
    T? GetSection<T>(string section) where T : class, new();
    Task SetAsync(string key, string value, CancellationToken ct = default);
}
