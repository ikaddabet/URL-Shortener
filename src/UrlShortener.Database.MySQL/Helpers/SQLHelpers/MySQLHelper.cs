using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Text.RegularExpressions;
using UrlShortener.Core;
using UrlShortener.Core.Helpers.SQLHelpers;

namespace UrlShortener.Database.MySQL.Helpers.SQLHelpers;

public partial class MySQLHelper(ILogger<MySQLHelper> logger, UrlShortenerOptions options) : ISQLHelper
{
    [GeneratedRegex(@"^[a-zA-Z0-9_-]{1,64}$")]
    private static partial Regex DatabaseNameRegex();

    public bool CheckDatabaseName(string Name, bool ThrowException = true)
    {
        try
        {
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException("Database name cannot be null or empty.");

            if (!DatabaseNameRegex().IsMatch(Name))
                throw new ArgumentException("Invalid MySQL database name. It must be between 1 and 64 characters and can only contain letters, numbers, underscores, and hyphens.");

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
            using MySqlConnection connection = new(options.ConnectionString);

            await connection.OpenAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking connection.");
            if (ThrowException)
                throw;
            return false;
        }
    }

    public async Task<bool> CheckTableExistsAsync(string tableName, CancellationToken cancellationToken = default, bool ThrowException = true)
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
            if (ThrowException)
                throw;
            return false;
        }
    }

    public async Task<bool> CreateTableAsync(string tableName, CancellationToken cancellationToken = default, bool ThrowException = true)
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
            if (ThrowException)
                throw;
            return false;
        }
    }

    public async Task<string> GetDatabaseVersionAsync(CancellationToken cancellationToken = default, bool ThrowException = true)
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
            if (ThrowException)
                throw;
            return string.Empty;
        }
    }
}
