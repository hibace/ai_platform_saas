using Microsoft.Extensions.Options;

namespace AiPlatform.ControlPlane;

public class LicenseService : ILicenseService
{
    private readonly LicenseOptions _options;

    public LicenseService(IOptions<LicenseOptions> options)
    {
        _options = options.Value;
    }

    public LicenseStatus GetStatus()
    {
        var valid = !string.IsNullOrEmpty(_options.Edition);
        var expires = _options.ExpiresAt;
        if (expires.HasValue && expires.Value < DateTime.UtcNow)
            valid = false;
        return new LicenseStatus(
            valid,
            _options.Edition,
            _options.ExpiresAt,
            _options.MaxAgents,
            _options.MaxTenants,
            (IReadOnlyList<string>?)_options.Features ?? Array.Empty<string>());
    }

    public bool IsFeatureEnabled(string featureKey)
    {
        var status = GetStatus();
        return status.Valid && (status.Features?.Contains(featureKey, StringComparer.OrdinalIgnoreCase) == true);
    }
}

public class LicenseOptions
{
    public const string SectionName = "License";
    public string? Edition { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxAgents { get; set; }
    public int? MaxTenants { get; set; }
    public List<string>? Features { get; set; }
}
