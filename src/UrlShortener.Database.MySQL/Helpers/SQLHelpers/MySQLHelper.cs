using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Text.RegularExpressions;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Helpers.SQLHelpers;
using UrlShortener.Core.Migrations;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.MySQL.Helpers.SQLHelpers;

public partial class MySQLHelper(ILogger<MySQLHelper> logger, IOptions<UrlShortenerOptions> options) : ISQLHelper
{
    [GeneratedRegex(@"^[a-zA-Z0-9_-]{1,64}$")]
    private static partial Regex DatabaseNameRegex();

    public void CheckDatabaseName()
    {
        try
        {
            if (string.IsNullOrEmpty(options.Value.DatabaseName))
                throw new ArgumentException("Database name cannot be null or empty.");

            if (!DatabaseNameRegex().IsMatch(options.Value.DatabaseName))
                throw new ArgumentException("Invalid MySQL database name. It must start with a letter and be under 64 characters.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Database name is invalid.");
            throw;
        }
    }

    public async Task CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new MySqlConnection(options.Value.ConnectionString);
            await connection.OpenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to establish a database connection.");
            throw;
        }
    }

    public void AddMigrations()
    {
        ShortenedUrlMigrationTracker.AddMigration(
            MigrationName: "Add Migration Table",
            TableNameWithPrefix: TableNames.MigrationsPrefixed(options.Value.TablePrefix),
            QueryCheckBeforeRun: $@"
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_name = '{TableNames.MigrationsPrefixed(options.Value.TablePrefix)}' AND table_schema = DATABASE();
        ",
            Query: $@"
            CREATE TABLE {TableNames.MigrationsPrefixed(options.Value.TablePrefix)} (
                MigrationName VARCHAR(255) NOT NULL PRIMARY KEY,
                AppliedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
        "
        );

        ShortenedUrlMigrationTracker.AddMigration(
            MigrationName: "Add ShortenedUrl Table",
            TableNameWithPrefix: TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix),
            QueryCheckBeforeRun: $@"
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_name = '{TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)}' AND table_schema = DATABASE();
        ",
            Query: $@"
            CREATE TABLE {TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)} (
                Id CHAR(36) NOT NULL DEFAULT (UUID()),  -- Auto-generated UUID
                LongUrl TEXT NOT NULL,
                ShortUrl VARCHAR(255) NOT NULL UNIQUE,
                Code VARCHAR(50) NOT NULL UNIQUE,
                CreatedOnUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (Id)
            );
        "
        );
    }

    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var Migration in ShortenedUrlMigrationTracker.Migrations)
        {
            try
            {
                var exists = await CheckMigartionExistsAsync(Migration, cancellationToken);
                if (exists) continue;
                logger?.LogInformation($"Applying migration '{Migration.MigrationName}'...");

                using var connection = new MySqlConnection(options.Value.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                using var transaction = await connection.BeginTransactionAsync(cancellationToken);
                var sqlTransaction = (MySqlTransaction)transaction; // Explicit cast

                try
                {
                    using var command = new MySqlCommand(Migration.Query, connection, sqlTransaction);
                    await command.ExecuteNonQueryAsync(cancellationToken);

                    var logMigrationQuery = $@"
                    INSERT INTO {TableNames.MigrationsPrefixed(options.Value.TablePrefix)}
                    (MigrationName, AppliedAt)
                    VALUES (@MigrationName, CURRENT_TIMESTAMP);
                ";
                    using var logCommand = new MySqlCommand(logMigrationQuery, connection, sqlTransaction);
                    logCommand.Parameters.AddWithValue("@MigrationName", Migration.MigrationName);
                    await logCommand.ExecuteNonQueryAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    logger?.LogInformation($"Migration '{Migration.MigrationName}' applied successfully.");
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error applying migration.");
                throw;
            }
        }
    }

    private async Task<bool> CheckMigartionExistsAsync(ShortenedUrlMigrationWithQuery Migration, CancellationToken cancellationToken = default)
    {
        try
        {
            // If the QueryCheckBeforeRun is null or empty, return true
            // This is useful for migrations that don't need to be checked
            if (string.IsNullOrWhiteSpace(Migration.QueryCheckBeforeRun))
                return true;

            using var connection = new MySqlConnection(options.Value.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            using var command = new MySqlCommand(Migration.QueryCheckBeforeRun, connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            var boolResult = result != null && Convert.ToInt32(result) > 0;
            if (!boolResult)
            {
                logger?.LogWarning($"Migration '{Migration.MigrationName}' does not exist.");
            }
            return boolResult;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error checking migration existence.");
            throw;
        }
    }

}
