namespace AiPlatform.Rag;

/// <summary>
/// In-memory RAG for minimal deployment. Production can use MongoDB Atlas Vector Search / pgvector / Qdrant.
/// </summary>
public class RagService : IRagService
{
    private readonly Dictionary<string, List<RagDocument>> _collections = new();
    private readonly object _lock = new();

    public Task CreateCollectionAsync(string collectionId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_collections.ContainsKey(collectionId))
                _collections[collectionId] = new List<RagDocument>();
        }
        return Task.CompletedTask;
    }

    public Task DeleteCollectionAsync(string collectionId, CancellationToken ct = default)
    {
        lock (_lock) _collections.Remove(collectionId);
        return Task.CompletedTask;
    }

    public Task IndexAsync(string collectionId, RagDocument doc, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_collections.TryGetValue(collectionId, out var list))
            {
                list = new List<RagDocument>();
                _collections[collectionId] = list;
            }
            var idx = list.FindIndex(d => d.Id == doc.Id);
            if (idx >= 0) list[idx] = doc;
            else list.Add(doc);
        }
        return Task.CompletedTask;
    }

    public Task IndexBatchAsync(string collectionId, IReadOnlyList<RagDocument> docs, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_collections.TryGetValue(collectionId, out var list))
            {
                list = new List<RagDocument>();
                _collections[collectionId] = list;
            }
            foreach (var doc in docs)
            {
                var idx = list.FindIndex(d => d.Id == doc.Id);
                if (idx >= 0) list[idx] = doc;
                else list.Add(doc);
            }
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RagSearchResult>> SearchAsync(string collectionId, string query, int topK = 5, CancellationToken ct = default)
    {
        List<RagSearchResult> results;
        lock (_lock)
        {
            if (!_collections.TryGetValue(collectionId, out var list))
                return Task.FromResult<IReadOnlyList<RagSearchResult>>(Array.Empty<RagSearchResult>());
            var scored = list
                .Select(d => (doc: d, score: SimpleScore(d.Content, query)))
                .Where(x => x.score > 0)
                .OrderByDescending(x => x.score)
                .Take(topK)
                .Select(x => new RagSearchResult(x.doc.Id, x.doc.Content, x.score, x.doc.Metadata))
                .ToList();
            results = scored;
        }
        return Task.FromResult<IReadOnlyList<RagSearchResult>>(results);
    }

    private static double SimpleScore(string content, string query)
    {
        if (string.IsNullOrEmpty(query)) return 0;
        var count = 0;
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var w in words)
        {
            if (content.Contains(w, StringComparison.OrdinalIgnoreCase)) count++;
        }
        return words.Length > 0 ? (double)count / words.Length : 0;
    }
}
