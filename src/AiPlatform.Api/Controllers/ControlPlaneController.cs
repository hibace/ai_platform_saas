using AiPlatform.ControlPlane;
using Microsoft.AspNetCore.Mvc;

namespace AiPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/control-plane")]
public class ControlPlaneController : ControllerBase
{
    private readonly ILicenseService _license;
    private readonly IConfigService _config;

    public ControlPlaneController(ILicenseService license, IConfigService config)
    {
        _license = license;
        _config = config;
    }

    [HttpGet("license")]
    public ActionResult<LicenseStatus> GetLicense() => Ok(_license.GetStatus());

    [HttpGet("config")]
    public ActionResult<object> GetConfig([FromQuery] string? key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            var value = _config.Get(key);
            return Ok(new { key, value });
        }
        return Ok(new { message = "Use ?key= to get a config value" });
    }
}
