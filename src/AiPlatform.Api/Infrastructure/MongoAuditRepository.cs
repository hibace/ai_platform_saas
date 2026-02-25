using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using MongoDB.Driver;

namespace AiPlatform.Api.Infrastructure;

public class MongoAuditRepository : IAuditRepository
{
    private readonly IMongoCollection<AuditEvent> _col;

    public MongoAuditRepository(IMongoClient client)
    {
        var db = client.GetDatabase("aiplatform");
        _col = db.GetCollection<AuditEvent>("audit");
    }

    public Task AppendAsync(AuditEvent evt, CancellationToken ct = default) =>
        _col.InsertOneAsync(evt, cancellationToken: ct);

    public async Task<IReadOnlyList<AuditEvent>> QueryAsync(AuditQuery query, CancellationToken ct = default)
    {
        var f = Builders<AuditEvent>.Filter.Empty;
        if (!string.IsNullOrEmpty(query.TenantId))
            f &= Builders<AuditEvent>.Filter.Eq(x => x.TenantId, query.TenantId);
        if (!string.IsNullOrEmpty(query.AgentId))
            f &= Builders<AuditEvent>.Filter.Eq(x => x.AgentId, query.AgentId);
        if (!string.IsNullOrEmpty(query.EventType))
            f &= Builders<AuditEvent>.Filter.Eq(x => x.EventType, query.EventType);
        if (query.From.HasValue)
            f &= Builders<AuditEvent>.Filter.Gte(x => x.Timestamp, query.From.Value);
        if (query.To.HasValue)
            f &= Builders<AuditEvent>.Filter.Lte(x => x.Timestamp, query.To.Value);

        var cursor = await _col.Find(f)
            .SortByDescending(x => x.Timestamp)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct).ConfigureAwait(false);
        return cursor;
    }
}
