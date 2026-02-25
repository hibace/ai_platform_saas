namespace AiPlatform.Rag;

public interface IRagService
{
    Task IndexAsync(string collectionId, RagDocument doc, CancellationToken ct = default);
    Task IndexBatchAsync(string collectionId, IReadOnlyList<RagDocument> docs, CancellationToken ct = default);
    Task<IReadOnlyList<RagSearchResult>> SearchAsync(string collectionId, string query, int topK = 5, CancellationToken ct = default);
    Task CreateCollectionAsync(string collectionId, CancellationToken ct = default);
    Task DeleteCollectionAsync(string collectionId, CancellationToken ct = default);
}

public record RagDocument(string Id, string Content, IReadOnlyDictionary<string, string>? Metadata = null);

public record RagSearchResult(string DocumentId, string Content, double Score, IReadOnlyDictionary<string, string>? Metadata = null);
