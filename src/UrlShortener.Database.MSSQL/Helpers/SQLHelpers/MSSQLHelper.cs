using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using UrlShortener.Core;
using UrlShortener.Core.Helpers.SQLHelpers;

namespace UrlShortener.Database.MSSQL.Helpers.SQLHelpers;

public partial class MSSQLHelper(ILogger<MSSQLHelper> logger, UrlShortenerOptions options) : ISQLHelper
{
    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9_]{0,127}$")]
    private static partial Regex DatabaseNameRegex();

    public virtual bool CheckDatabaseName(string Name, bool ThrowException = true)
    {
        try
        {
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException("Database name cannot be null or empty.");

            if (!DatabaseNameRegex().IsMatch(Name))
                throw new ArgumentException("Invalid MSSQL database name. It must start with a letter and be under 128 characters.");

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Database name is invalid.");
            if (ThrowException)
                throw;
            return false;
        }
    }

    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default, bool ThrowException = true)
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
            if (ThrowException)
                throw;
            return false;
        }
    }

    public async Task<bool> CheckTableExistsAsync(string tableName, CancellationToken cancellationToken = default, bool ThrowException = true)
    {
        try
        {
            string tableNameWithPrefix = options.TablePrefix + tableName;

            using SqlConnection connection = new(options.ConnectionString);

            await connection.OpenAsync(cancellationToken);
            SqlCommand cmd = new SqlCommand("""
                SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableNameWithPrefix}'
            """, connection);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result != null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking table exists.");
            if (ThrowException)
                throw;
            return false;
        }
    }

    public async Task<bool> CreateTableAsync(string tableName, CancellationToken cancellationToken = default, bool ThrowException = true)
    {
        try
        {
            string tableNameWithPrefix = options.TablePrefix + tableName;

            using var connection = new SqlConnection(options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand("""
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
            if (ThrowException)
                throw;
            return false;
        }
    }

    public async Task<string> GetDatabaseVersionAsync(CancellationToken cancellationToken = default, bool ThrowException = true)
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
            if (ThrowException)
                throw;
            return string.Empty;
        }
    }

}
