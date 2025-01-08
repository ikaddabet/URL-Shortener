﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Helpers.SQLHelpers;
using UrlShortener.Core.Migrations;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.MSSQL.Helpers.SQLHelpers;

public partial class MSSQLHelper(ILogger<MSSQLHelper> logger, UrlShortenerOptions options) : ISQLHelper
{
    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9_]{0,127}$")]
    private static partial Regex DatabaseNameRegex();

    public void CheckDatabaseName()
    {
        try
        {
            if (string.IsNullOrEmpty(options.DatabaseName))
                throw new ArgumentException("Database name cannot be null or empty.");

            if (!DatabaseNameRegex().IsMatch(options.DatabaseName))
                throw new ArgumentException("Invalid MSSQL database name. It must start with a letter and be under 128 characters.");
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
            using var connection = new SqlConnection(options.ConnectionString);
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
            TableNameWithPrefix: TableNames.MigrationsPrefixed(options.TablePrefix),
            QueryCheckBeforeExectution: $@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.tables
                    WHERE name = '{TableNames.MigrationsPrefixed(options.TablePrefix)}'
                )
                    SELECT CAST(1 AS BIT) AS TableExists; -- TRUE
                ELSE
                    SELECT CAST(0 AS BIT) AS TableExists; -- FALSE
            ",
            Query: $@"
                CREATE TABLE {TableNames.MigrationsPrefixed(options.TablePrefix)} (
                    MigrationName NVARCHAR(255) NOT NULL PRIMARY KEY,
                    AppliedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );
            "
        );

        ShortenedUrlMigrationTracker.AddMigration(
            MigrationName: "Add ShortenedUrl Table",
            TableNameWithPrefix: TableNames.ShortenedUrlPrefixed(options.TablePrefix),
            QueryCheckBeforeExectution: $@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.tables
                    WHERE name = '{TableNames.ShortenedUrlPrefixed(options.TablePrefix)}'
                )
                    SELECT CAST(1 AS BIT) AS TableExists; -- TRUE
                ELSE
                    SELECT CAST(0 AS BIT) AS TableExists; -- FALSE
            ",
            Query: $@"
                CREATE TABLE {TableNames.ShortenedUrlPrefixed(options.TablePrefix)} (
                    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),  -- Auto-generated GUID
                    LongUrl NVARCHAR(MAX) NOT NULL,
                    ShortUrl NVARCHAR(255) NOT NULL UNIQUE,
                    Code NVARCHAR(50) NOT NULL UNIQUE,
                    CreatedOnUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
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

                using var connection = new SqlConnection(options.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                // Cast DbTransaction to SqlTransaction
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);
                var sqlTransaction = (SqlTransaction)transaction; // Explicit cast

                try
                {
                    using var command = new SqlCommand(Migration.Query, connection, sqlTransaction);
                    await command.ExecuteNonQueryAsync(cancellationToken);

                    var logMigrationQuery = $@"
                        INSERT INTO {TableNames.MigrationsPrefixed(options.TablePrefix)}
                        (MigrationName, AppliedAt)
                        VALUES (@MigrationName, GETUTCDATE());
                    ";
                    using var logCommand = new SqlCommand(logMigrationQuery, connection, sqlTransaction);
                    logCommand.Parameters.AddWithValue("@MigrationName", Migration.MigrationName);
                    await logCommand.ExecuteNonQueryAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
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
            // If the QueryCheckBeforeExectution is null or empty, return true
            // This is useful for migrations that don't need to be checked
            if (string.IsNullOrWhiteSpace(Migration.QueryCheckBeforeExectution))
                return true;

            using var connection = new SqlConnection(options.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            using var command = new SqlCommand(Migration.QueryCheckBeforeExectution, connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            var boolResult = result != null && (bool)result;
            if (!boolResult)
            {
                logger?.LogInformation($"Migration '{Migration.MigrationName}' does not exist.");
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