namespace UrlShortener.Core.Helpers.SQLHelpers;

/// <summary>
/// Provides helper methods for interacting with SQL databases.
/// </summary>
public interface ISQLHelper
{
    void CheckDatabaseName();
    Task CheckConnectionAsync(CancellationToken cancellationToken = default);

    void AddMigrations();
    Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);
}
