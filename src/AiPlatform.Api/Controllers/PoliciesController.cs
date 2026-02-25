using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AiPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/policies")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyRepository _policies;

    public PoliciesController(IPolicyRepository policies) => _policies = policies;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Policy>>> List([FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var list = await _policies.ListAsync(tenantId, ct).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Policy>> Get(Guid id, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var policy = await _policies.GetByIdAsync(id, tenantId, ct).ConfigureAwait(false);
        if (policy == null) return NotFound();
        return Ok(policy);
    }

    [HttpPost]
    [HttpPut]
    public async Task<ActionResult<Policy>> Save([FromBody] SavePolicyRequest req, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var policy = new Policy
        {
            Id = req.Id ?? Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description,
            TenantId = tenantId,
            Rules = (req.Rules ?? Array.Empty<PolicyRuleRequest>()).Select(r => new PolicyRule
            {
                Action = r.Action ?? "allow",
                Resource = r.Resource,
                ToolPattern = r.ToolPattern,
                Conditions = r.Conditions
            }).ToArray()
        };
        policy = await _policies.SaveAsync(policy, ct).ConfigureAwait(false);
        return Ok(policy);
    }
}

public record SavePolicyRequest(Guid? Id, string Name, string? Description, PolicyRuleRequest[]? Rules);
public record PolicyRuleRequest(string? Action, string? Resource, string? ToolPattern, Dictionary<string, string>? Conditions);
