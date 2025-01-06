namespace UrlShortener.DatabaseInitializer;
public interface IUrlShortenerDatabaseInitializer
{
    Task ValidateConnectionAsync(CancellationToken cancellationToken = default);
    Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
}
