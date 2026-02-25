using System.Linq;
using AiPlatform.Agents;
using AiPlatform.Agents.Llm;
using AiPlatform.Agents.Tools;
using AiPlatform.Api.Infrastructure;
using AiPlatform.ControlPlane;
using AiPlatform.Core.Repositories;
using AiPlatform.Rag;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL (agents, policies, tools)
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres") ?? "Host=localhost;Database=aiplatform;Username=postgres;Password=postgres"));

// MongoDB (audit)
var mongoUrl = builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUrl));
builder.Services.AddSingleton<IAuditRepository, MongoAuditRepository>();

// Redis (optional cache/sessions)
var redis = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redis))
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redis);

// Kafka (optional event bus for audit)
// builder.Services.AddSingleton<IAuditPublisher, KafkaAuditPublisher>();

builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection(LlmOptions.SectionName));
builder.Services.Configure<LicenseOptions>(builder.Configuration.GetSection(LicenseOptions.SectionName));

builder.Services.AddHttpClient("llm");
builder.Services.AddSingleton<ILlmClientFactory, LlmClientFactory>();
builder.Services.AddSingleton<IToolExecutor>(sp => new EchoTool());
builder.Services.AddSingleton<IToolExecutor>(sp => new HttpGetTool(sp.GetRequiredService<IHttpClientFactory>()));
builder.Services.AddSingleton<IToolExecutorFactory, ToolExecutorFactory>();
builder.Services.AddSingleton<IAuditService, AuditService>();

// Scoped: everything that depends on AppDbContext (DbContext is Scoped by default)
builder.Services.AddScoped<IPolicyRepository, EfPolicyRepository>();
builder.Services.AddScoped<IAgentRepository, EfAgentRepository>();
builder.Services.AddScoped<IToolRegistry, EfToolRegistry>();
// Explicitly Scoped: otherwise something may be Singleton and AppDbContext cannot be injected
var policyEvaluatorDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IPolicyEvaluator));
if (policyEvaluatorDescriptor != null)
    builder.Services.Remove(policyEvaluatorDescriptor);
builder.Services.AddScoped<IPolicyEvaluator, PolicyEvaluator>();
builder.Services.AddScoped<IAgentRuntime, AgentRuntime>();

builder.Services.AddSingleton<IRagService, RagService>();
builder.Services.AddSingleton<ILicenseService, LicenseService>();
builder.Services.AddSingleton<IConfigService, ConfigService>();

builder.Services.AddHostedService<DatabaseInitializerHostedService>();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AI Platform API", Version = "v1" });
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
