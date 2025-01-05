using Microsoft.Data.SqlClient;
using UrlShortener.Core.DatabaseInitializer;
using UrlShortener.Core.Helpers.SQLHelper;

namespace UrlShortener.Database.MSSQL.DatabaseInitializer;

public class MSSQLUrlShortenerDatabaseInitializer(ISQLHelper sqlHelper) : IUrlShortenerDatabaseInitializer
{
    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await sqlHelper.ExecuteQueryAsync("""
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'UrlShortenerDB')
                BEGIN CREATE DATABASE UrlShortenerDB END
            """, cancellationToken);
    }
    public async Task InitializeSchemaAsync(CancellationToken cancellationToken = default)
    {
        await sqlHelper.ExecuteQueryAsync("""
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ShortUrls') 
            BEGIN 
            CREATE TABLE ShortUrls ( 
                Id INT PRIMARY KEY IDENTITY, 
                OriginalUrl NVARCHAR(2048) NOT NULL, 
                ShortUrl NVARCHAR(50) NOT NULL, 
                CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
                ) 
            END
            """, cancellationToken);
    }
}
