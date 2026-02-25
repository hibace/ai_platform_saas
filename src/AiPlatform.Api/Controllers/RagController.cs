using AiPlatform.Rag;
using Microsoft.AspNetCore.Mvc;

namespace AiPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/rag")]
public class RagController : ControllerBase
{
    private readonly IRagService _rag;

    public RagController(IRagService rag) => _rag = rag;

    [HttpPost("collections/{collectionId}")]
    public async Task<ActionResult> CreateCollection(string collectionId, CancellationToken ct = default)
    {
        await _rag.CreateCollectionAsync(collectionId, ct).ConfigureAwait(false);
        return NoContent();
    }

    [HttpDelete("collections/{collectionId}")]
    public async Task<ActionResult> DeleteCollection(string collectionId, CancellationToken ct = default)
    {
        await _rag.DeleteCollectionAsync(collectionId, ct).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("collections/{collectionId}/documents")]
    public async Task<ActionResult> Index(string collectionId, [FromBody] IndexDocumentRequest req, CancellationToken ct = default)
    {
        await _rag.IndexAsync(collectionId, new RagDocument(req.Id, req.Content, req.Metadata), ct).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("collections/{collectionId}/documents/batch")]
    public async Task<ActionResult> IndexBatch(string collectionId, [FromBody] IndexDocumentRequest[] docs, CancellationToken ct = default)
    {
        var list = docs.Select(d => new RagDocument(d.Id, d.Content, d.Metadata)).ToList();
        await _rag.IndexBatchAsync(collectionId, list, ct).ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("collections/{collectionId}/search")]
    public async Task<ActionResult<IReadOnlyList<RagSearchResult>>> Search(string collectionId, [FromQuery] string q, [FromQuery] int topK = 5, CancellationToken ct = default)
    {
        var results = await _rag.SearchAsync(collectionId, q, topK, ct).ConfigureAwait(false);
        return Ok(results);
    }
}

public record IndexDocumentRequest(string Id, string Content, IReadOnlyDictionary<string, string>? Metadata);
