namespace AiPlatform.ControlPlane;

public interface ILicenseService
{
    LicenseStatus GetStatus();
    bool IsFeatureEnabled(string featureKey);
}

public record LicenseStatus(
    bool Valid,
    string? Edition,
    DateTime? ExpiresAt,
    int? MaxAgents,
    int? MaxTenants,
    IReadOnlyList<string> Features);
