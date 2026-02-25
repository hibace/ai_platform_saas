using AiPlatform.Core.Repositories;
using AiPlatform.Rag;
using Microsoft.EntityFrameworkCore;

namespace AiPlatform.Api.Infrastructure;

/// <summary>
/// Runs EnsureCreated and DB seeding after app start in a separate scope.
/// Avoids dependency resolution issues when initializing from Program.cs.
/// </summary>
public sealed class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializerHostedService> _logger;

    public DatabaseInitializerHostedService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

                var audit = scope.ServiceProvider.GetService<IAuditRepository>();
                var rag = scope.ServiceProvider.GetService<IRagService>();
                var seeder = new DataSeeder(db, audit, rag);
                await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);
            }

            _logger.LogInformation("Database initialized and seeded.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
