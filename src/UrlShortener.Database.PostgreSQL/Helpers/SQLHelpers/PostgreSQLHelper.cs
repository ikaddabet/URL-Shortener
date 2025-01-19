using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Text.RegularExpressions;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Helpers.SQLHelpers;
using UrlShortener.Core.Migrations;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.PostgreSQL.Helpers.SQLHelpers;

public partial class PostgreSQLHelper(ILogger<PostgreSQLHelper> logger, IOptions<UrlShortenerOptions> options) : ISQLHelper
{
    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9_]{0,127}$")]
    private static partial Regex DatabaseNameRegex();

    public void CheckDatabaseName()
    {
        try
        {
            if (string.IsNullOrEmpty(options.Value.DatabaseName))
                throw new ArgumentException("Database name cannot be null or empty.");

            if (!DatabaseNameRegex().IsMatch(options.Value.DatabaseName))
                throw new ArgumentException("Invalid PostgreSQL database name. It must start with a letter and be under 128 characters.");
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
            using var connection = new NpgsqlConnection(options.Value.ConnectionString);
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
        // Add migration for the migration table
        ShortenedUrlMigrationTracker.AddMigration(
            MigrationName: "Add Migration Table",
            TableNameWithPrefix: TableNames.MigrationsPrefixed(options.Value.TablePrefix),
            QueryCheckBeforeRun: $@"
                    SELECT EXISTS (
                        SELECT 1
                        FROM information_schema.tables
                        WHERE table_name = '{TableNames.MigrationsPrefixed(options.Value.TablePrefix)}'
                    );
                ",
            Query: $@"
                    CREATE TABLE {TableNames.MigrationsPrefixed(options.Value.TablePrefix)} (
                        MigrationName VARCHAR(255) PRIMARY KEY,
                        AppliedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );
                "
        );

        // Add migration for the shortened URL table
        ShortenedUrlMigrationTracker.AddMigration(
            MigrationName: "Add ShortenedUrl Table",
            TableNameWithPrefix: TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix),
            QueryCheckBeforeRun: $@"
                    SELECT EXISTS (
                        SELECT 1
                        FROM information_schema.tables
                        WHERE table_name = '{TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)}'
                    );
                ",
            Query: $@"
                    CREATE TABLE {TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)} (
                        Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                        OriginalUrl TEXT NOT NULL,
                        ShortUrl VARCHAR(255) NOT NULL UNIQUE,
                        Code VARCHAR(50) NOT NULL UNIQUE,
                        CreatedOnUtc TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
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
                var exists = await CheckMigrationExistsAsync(Migration, cancellationToken);
                if (exists) continue;

                logger?.LogInformation("Applying migration '{MigrationName}'...", Migration.MigrationName);

                using var connection = new NpgsqlConnection(options.Value.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                try
                {
                    await connection.ExecuteAsync(Migration.Query!, transaction: transaction);

                    var logMigrationQuery = $@"
                            INSERT INTO {TableNames.MigrationsPrefixed(options.Value.TablePrefix)} 
                            (MigrationName, AppliedAt)
                            VALUES (@MigrationName, CURRENT_TIMESTAMP);
                        ";
                    await connection.ExecuteAsync(logMigrationQuery, new { Migration.MigrationName }, transaction);

                    await transaction.CommitAsync(cancellationToken);
                    logger?.LogInformation("Migration '{MigrationName}' applied successfully.", Migration.MigrationName);
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

    private async Task<bool> CheckMigrationExistsAsync(ShortenedUrlMigrationWithQuery Migration, CancellationToken cancellationToken = default)
    {
        try
        {
            // If the QueryCheckBeforeRun is null or empty, return true
            if (string.IsNullOrWhiteSpace(Migration.QueryCheckBeforeRun))
                return true;

            using var connection = new NpgsqlConnection(options.Value.ConnectionString);

            var result = await connection.ExecuteScalarAsync<bool>(Migration.QueryCheckBeforeRun);

            if (!result)
            {
                logger?.LogWarning("Migration '{MigrationName}' does not exist.", Migration.MigrationName);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error checking migration existence.");
            throw;
        }
    }
}
