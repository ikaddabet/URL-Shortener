using UrlShortener.Core.Helpers.SQLHelpers;

namespace UrlShortener.DatabaseInitializer;
public class UrlShortenerDatabaseInitializer(ISQLHelper sqlHelper) : IUrlShortenerDatabaseInitializer
{
    public async Task ValidateConnectionAsync(CancellationToken cancellationToken = default)
    {
        sqlHelper.CheckDatabaseName();
        await sqlHelper.CheckConnectionAsync(cancellationToken);
    }

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        sqlHelper.AddMigrations();
        await sqlHelper.ApplyMigrationsAsync(cancellationToken);
    }
}
