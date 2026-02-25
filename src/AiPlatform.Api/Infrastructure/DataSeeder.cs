using AiPlatform.Core.Domain;
using AiPlatform.Core.Repositories;
using AiPlatform.Rag;
using Microsoft.EntityFrameworkCore;

namespace AiPlatform.Api.Infrastructure;

/// <summary>
/// Seeds the database and stores with initial data on first run.
/// </summary>
public class DataSeeder
{
    private readonly AppDbContext _db;
    private readonly IAuditRepository? _audit;
    private readonly IRagService? _rag;

    public static readonly Guid SeedPolicyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SeedAgentId1 = Guid.Parse("22222222-2222-2222-2222-222222222221");
    public static readonly Guid SeedAgentId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public const string ToolIdEcho = "echo";
    public const string ToolIdHttpGet = "http_get";
    public const string RagCollectionDocs = "docs";

    public DataSeeder(AppDbContext db, IAuditRepository? audit = null, IRagService? rag = null)
    {
        _db = db;
        _audit = audit;
        _rag = rag;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedToolsAsync(ct).ConfigureAwait(false);
        await SeedPoliciesAsync(ct).ConfigureAwait(false);
        await SeedAgentsAsync(ct).ConfigureAwait(false);
        await SeedAuditAsync(ct).ConfigureAwait(false);
        await SeedRagAsync(ct).ConfigureAwait(false);
    }

    private async Task SeedToolsAsync(CancellationToken ct)
    {
        if (await _db.Tools.AnyAsync(ct).ConfigureAwait(false)) return;

        _db.Tools.AddRange(
            new ToolEntity
            {
                Id = ToolIdEcho,
                Name = "Echo",
                Description = "Возвращает введённый текст без изменений (для тестов).",
                InputSchema = "{\"type\":\"object\",\"properties\":{\"message\":{\"type\":\"string\"}}}",
                TenantId = null,
                IsEnabled = true
            },
            new ToolEntity
            {
                Id = ToolIdHttpGet,
                Name = "HTTP GET",
                Description = "Выполняет GET-запрос по указанному URL.",
                InputSchema = "{\"type\":\"object\",\"properties\":{\"url\":{\"type\":\"string\"}}}",
                TenantId = null,
                IsEnabled = true
            },
            new ToolEntity
            {
                Id = "weather",
                Name = "Погода",
                Description = "Получить текущую погоду по городу (демо).",
                InputSchema = "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}}}",
                TenantId = null,
                IsEnabled = true
            }
        );
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task SeedPoliciesAsync(CancellationToken ct)
    {
        if (await _db.Policies.AnyAsync(ct).ConfigureAwait(false)) return;

        var now = DateTime.UtcNow;
        _db.Policies.Add(new PolicyEntity
        {
            Id = SeedPolicyId,
            Name = "Политика по умолчанию",
            Description = "Разрешает вызов всех инструментов для демонстрации.",
            TenantId = null,
            CreatedAt = now,
            UpdatedAt = now,
            Rules = new List<PolicyRuleEntity>
            {
                new() { Action = "allow", Resource = "*", ToolPattern = "*", Conditions = null }
            }
        });
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task SeedAgentsAsync(CancellationToken ct)
    {
        if (await _db.Agents.AnyAsync(ct).ConfigureAwait(false)) return;

        var now = DateTime.UtcNow;
        _db.Agents.AddRange(
            new AgentEntity
            {
                Id = SeedAgentId1,
                Name = "Помощник",
                Description = "Универсальный агент с инструментами Echo и HTTP GET.",
                SystemPrompt = "Ты полезный помощник. Отвечай кратко и по делу.",
                ToolIds = new List<string> { ToolIdEcho, ToolIdHttpGet },
                RagCollectionId = null,
                PolicyId = SeedPolicyId.ToString(),
                LlmProvider = "openai",
                LlmModel = "gpt-4o-mini",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now,
                TenantId = null
            },
            new AgentEntity
            {
                Id = SeedAgentId2,
                Name = "Эхо-агент",
                Description = "Простой агент только с инструментом Echo.",
                SystemPrompt = "Ты эхо-агент. Используй инструмент echo для ответа.",
                ToolIds = new List<string> { ToolIdEcho },
                RagCollectionId = RagCollectionDocs,
                PolicyId = SeedPolicyId.ToString(),
                LlmProvider = "openai",
                LlmModel = "gpt-4o-mini",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now,
                TenantId = null
            }
        );
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task SeedAuditAsync(CancellationToken ct)
    {
        if (_audit == null) return;

        var query = new AuditQuery(null, null, null, null, null, 0, 1);
        var existing = await _audit.QueryAsync(query, ct).ConfigureAwait(false);
        if (existing.Count > 0) return;

        var baseTime = DateTime.UtcNow.AddHours(-2);
        var events = new[]
        {
            new AuditEvent
            {
                Id = Guid.NewGuid(),
                EventType = "agent.run.started",
                AgentId = SeedAgentId1.ToString(),
                UserId = "user-demo",
                Timestamp = baseTime,
                Payload = "{\"message\":\"Привет\"}"
            },
            new AuditEvent
            {
                Id = Guid.NewGuid(),
                EventType = "agent.run.completed",
                AgentId = SeedAgentId1.ToString(),
                UserId = "user-demo",
                Timestamp = baseTime.AddSeconds(5),
                Result = "Привет! Чем могу помочь?"
            },
            new AuditEvent
            {
                Id = Guid.NewGuid(),
                EventType = "tool.invoked",
                AgentId = SeedAgentId1.ToString(),
                ToolName = ToolIdEcho,
                Timestamp = baseTime.AddSeconds(2),
                Payload = "{\"message\":\"Привет\"}"
            }
        };

        foreach (var evt in events)
            await _audit.AppendAsync(evt, ct).ConfigureAwait(false);
    }

    private async Task SeedRagAsync(CancellationToken ct)
    {
        if (_rag == null) return;

        await _rag.CreateCollectionAsync(RagCollectionDocs, ct).ConfigureAwait(false);
        var docs = new[]
        {
            new RagDocument("doc1", "AI Platform — это self-hosted платформа для запуска AI-агентов. Поддерживаются инструменты, RAG и политики доступа.", new Dictionary<string, string> { ["source"] = "readme" }),
            new RagDocument("doc2", "Агенты настраиваются через System Prompt и список инструментов. Можно подключить OpenAI-совместимые LLM.", new Dictionary<string, string> { ["source"] = "docs" }),
            new RagDocument("doc3", "RAG позволяет индексировать документы и искать по ним при запросах к агенту.", new Dictionary<string, string> { ["source"] = "docs" })
        };
        await _rag.IndexBatchAsync(RagCollectionDocs, docs, ct).ConfigureAwait(false);
    }
}
