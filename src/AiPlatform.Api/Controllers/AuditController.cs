using AiPlatform.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AiPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditRepository _audit;

    public AuditController(IAuditRepository audit) => _audit = audit;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Core.Domain.AuditEvent>>> Query(
        [FromQuery] string? tenantId,
        [FromQuery] string? agentId,
        [FromQuery] string? eventType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var query = new AuditQuery(tenantId, agentId, eventType, from, to, skip, take);
        var list = await _audit.QueryAsync(query, ct).ConfigureAwait(false);
        return Ok(list);
    }
}
