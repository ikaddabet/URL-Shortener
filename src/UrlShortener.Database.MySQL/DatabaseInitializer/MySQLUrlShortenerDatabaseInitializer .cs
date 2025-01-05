using MySqlConnector;
using UrlShortener.Core.DatabaseInitializer;
using UrlShortener.Core.Helpers.SQLHelper;

namespace UrlShortener.Database.MySQL.DatabaseInitializer;

public class MySQLUrlShortenerDatabaseInitializer(ISQLHelper sqlHelper) : IUrlShortenerDatabaseInitializer
{
    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await sqlHelper.ExecuteQueryAsync("""
            CREATE DATABASE IF NOT EXISTS UrlShortenerDB
            """, cancellationToken);
    }

    public async Task InitializeSchemaAsync(CancellationToken cancellationToken = default)
    {
        await sqlHelper.ExecuteQueryAsync("""
            CREATE TABLE IF NOT EXISTS ShortUrls (
                Id INT PRIMARY KEY AUTO_INCREMENT,
                OriginalUrl VARCHAR(2048) NOT NULL,
                ShortUrl VARCHAR(50) NOT NULL,
                CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            )
            """, cancellationToken);
    }
}
