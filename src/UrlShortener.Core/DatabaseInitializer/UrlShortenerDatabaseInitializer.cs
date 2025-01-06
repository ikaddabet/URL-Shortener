using UrlShortener.Core.Helpers.SQLHelpers;

namespace UrlShortener.Core.DatabaseInitializer;
internal class UrlShortenerDatabaseInitializer(ISQLHelper sqlHelper) : IUrlShortenerDatabaseInitializer
{
    public async Task ValidateConnectionAsync(CancellationToken cancellationToken = default)
    {
        sqlHelper.CheckDatabaseNameAsync();
        await sqlHelper.CheckConnectionAsync(cancellationToken);
    }

    public Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task InitializeSchemaAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
