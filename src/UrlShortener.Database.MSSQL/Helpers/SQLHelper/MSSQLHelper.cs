using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using UrlShortener.Core;
using UrlShortener.Core.Helpers.SQLHelper;

namespace UrlShortener.Database.MSSQL.Helpers.SQLHelper;

public class MSSQLHelper(ILogger<MSSQLHelper> logger, UrlShortenerOptions options) : ISQLHelper
{
    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using SqlConnection connection = new(options.ConnectionString);

            await connection.OpenAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Connection cannot be established.");
            return false;
        }
    }

    public async Task<bool> CheckTableExistsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        try
        {
            string tableNameWithPrefix = options.TablePrefix + tableName;

            using SqlConnection connection = new(options.ConnectionString);

            await connection.OpenAsync(cancellationToken);
            SqlCommand cmd = new($"""
                SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableNameWithPrefix}'
            """, connection);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result != null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking table exists.");
            return false;
        }
    }

    public async Task<bool> CreateTableAsync(string tableName, CancellationToken cancellationToken = default)
    {
        try
        {
            string tableNameWithPrefix = options.TablePrefix + tableName;

            using var connection = new SqlConnection(options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand($"""
                CREATE TABLE {tableNameWithPrefix} (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Name NVARCHAR(100)
                );
                """, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation($"Table created successfully in SQL Server.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create table in SQL Server.");
            return false;
        }
    }

    public async Task<string> GetDatabaseVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using SqlConnection connection = new(options.ConnectionString);

            await connection.OpenAsync(cancellationToken);
            SqlCommand cmd = new("SELECT @@VERSION", connection);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result?.ToString() ?? throw new Exception("Failed to get database version.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting database version.");
            throw;
        }
    }
}
