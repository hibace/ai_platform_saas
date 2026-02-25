using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AiPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/tools")]
public class ToolsController : ControllerBase
{
    private readonly IToolRegistry _tools;

    public ToolsController(IToolRegistry tools) => _tools = tools;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ToolDefinition>>> List([FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var list = await _tools.ListAsync(tenantId, ct).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ToolDefinition>> Get(string id, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var tool = await _tools.GetByIdAsync(id, tenantId, ct).ConfigureAwait(false);
        if (tool == null) return NotFound();
        return Ok(tool);
    }

    [HttpPost]
    public async Task<ActionResult<ToolDefinition>> Register([FromBody] RegisterToolRequest req, [FromQuery] string? tenantId, CancellationToken ct = default)
    {
        var tool = new ToolDefinition
        {
            Id = req.Id ?? Guid.NewGuid().ToString("N"),
            Name = req.Name,
            Description = req.Description ?? "",
            InputSchema = req.InputSchema,
            TenantId = tenantId,
            IsEnabled = req.IsEnabled ?? true
        };
        tool = await _tools.RegisterAsync(tool, ct).ConfigureAwait(false);
        return CreatedAtAction(nameof(Get), new { id = tool.Id }, tool);
    }
}

public record RegisterToolRequest(string Name, string? Description, string? InputSchema, string? Id, bool? IsEnabled);
