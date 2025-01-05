using Microsoft.Extensions.Logging;
using MySqlConnector;
using UrlShortener.Core;
using UrlShortener.Core.Helpers.SQLHelper;

namespace UrlShortener.Database.MySQL.Helpers.SQLHelper;

public class MySQLHelper(ILogger<MySQLHelper> logger, UrlShortenerOptions options) : ISQLHelper
{
    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using MySqlConnection connection = new(options.ConnectionString);

            await connection.OpenAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking connection.");
            return false;
        }
    }

    public async Task<bool> CheckTableExistsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        try
        {
            using MySqlConnection connection = new(options.ConnectionString);

            await connection.OpenAsync(cancellationToken);
            MySqlCommand cmd = new($"SHOW TABLES LIKE '{tableName}'", connection);
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
            using var connection = new MySqlConnection(options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new MySqlCommand("""
                CREATE TABLE {tableName} (
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    Name VARCHAR(100)
                );
                """, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
            logger.LogInformation($"Table created successfully in MySQL.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create table in MySQL.");
            return false;
        }
    }

    public async Task<string> GetDatabaseVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using MySqlConnection connection = new(options.ConnectionString);

            await connection.OpenAsync(cancellationToken);
            MySqlCommand cmd = new("SELECT VERSION()", connection);
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
