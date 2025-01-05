namespace UrlShortener.Core.DatabaseInitializer;
public interface IUrlShortenerDatabaseInitializer
{
    Task ValidateConnectionAsync(CancellationToken cancellationToken = default);
    Task InitializeSchemaAsync(CancellationToken cancellationToken = default);
    Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
}
