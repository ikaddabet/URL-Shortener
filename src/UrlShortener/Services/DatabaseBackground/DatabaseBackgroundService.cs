using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UrlShortener.DatabaseInitializer;

namespace UrlShortener.Services.DatabaseBackground;
public class DatabaseBackgroundService(ILogger<DatabaseBackgroundService> logger, IUrlShortenerDatabaseInitializer databaseInitializer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await databaseInitializer.ValidateConnectionAsync(stoppingToken);

            await databaseInitializer.InitializeDatabaseAsync(stoppingToken);

            logger.LogInformation("Database initialization completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing database.");
        }
    }
}
