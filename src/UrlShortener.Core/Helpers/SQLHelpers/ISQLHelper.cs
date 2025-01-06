namespace UrlShortener.Core.Helpers.SQLHelpers;

public interface ISQLHelper
{
    bool CheckDatabaseNameAsync(bool ThrowException = true);
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default, bool ThrowException = true);
    Task<bool> CheckTableExistsAsync(CancellationToken cancellationToken = default, bool ThrowException = true);
    Task<bool> CheckMigartionExistsAsync(CancellationToken cancellationToken = default, bool ThrowException = true);
    Task<bool> CreateSchemaAsync(CancellationToken cancellationToken = default, bool ThrowException = true);
    Task<string> GetDatabaseVersionAsync(CancellationToken cancellationToken = default, bool ThrowException = true);
}
