namespace UrlShortener.Core.Helpers.SQLHelper;

public interface ISQLHelper
{
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckTableExistsAsync(string Name, CancellationToken cancellationToken = default);
    Task<bool> CreateTableAsync(string Name, CancellationToken cancellationToken = default)
    Task<string> GetDatabaseVersionAsync(CancellationToken cancellationToken = default);
}
