using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Helpers.SQLHelpers;
using UrlShortener.Core.Migrations;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.MongoDB.Helpers.SQLHelpers;

public partial class MongoDBHelper : ISQLHelper
{
    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9_]{0,127}$")]
    private static partial Regex DatabaseNameRegex();

    private readonly ILogger<MongoDBHelper> _logger;
    private readonly IOptions<UrlShortenerOptions> _options;
    private readonly IMongoDatabase _database;

    public MongoDBHelper(ILogger<MongoDBHelper> logger, IOptions<UrlShortenerOptions> options)
    {
        _logger = logger;
        _options = options;
        var client = new MongoClient(options.Value.ConnectionString);
        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    public void CheckDatabaseName()
    {
        try
        {
            if (string.IsNullOrEmpty(_database.DatabaseNamespace.DatabaseName))
                throw new ArgumentException("Database name cannot be null or empty.");

            if (!DatabaseNameRegex().IsMatch(_database.DatabaseNamespace.DatabaseName))
                throw new ArgumentException("Invalid MongoDB database name. It must start with a letter and be under 128 characters.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Database name is invalid.");
            throw;
        }
    }

    public async Task CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // ping MongoDB the server
            await _database.Client.ListDatabasesAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to establish a database connection.");
            throw;
        }
    }

    public void AddMigrations()
    {
        AddMigration(
            MigrationName: "Add Migration Collection",
            TableNameWithPrefix: TableNames.MigrationsPrefixed(_database.DatabaseNamespace.DatabaseName),
            QueryCheckBeforeRunExecution: async () =>
            {
                var migrationCollection = _database.GetCollection<BsonDocument>(TableNames.MigrationsPrefixed(_options.Value.TablePrefix));
                var count = await migrationCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
                return count > 0;
            },
            QueryExecution: async () =>
            {
                var migrationCollection = _database.GetCollection<BsonDocument>(TableNames.MigrationsPrefixed(_options.Value.TablePrefix));
                var document = new BsonDocument
                {
                    { "MigrationName", "Add Migration Collection" },
                    { "AppliedAt", DateTime.UtcNow }
                };
                await migrationCollection.InsertOneAsync(document);
                return true;
            },
            SaveToHistory: false
        );

        // Similar logic for "ShortenedUrl" migration, creating necessary collections or performing updates
        AddMigration(
            MigrationName: "Add ShortenedUrl Collection",
            TableNameWithPrefix: TableNames.ShortenedUrlPrefixed(_database.DatabaseNamespace.DatabaseName),
            QueryCheckBeforeRunExecution: async () =>
            {
                var shortenedUrlsCollection = _database.GetCollection<ShortenedUrlString>(TableNames.ShortenedUrlPrefixed(_options.Value.TablePrefix));

                // Check if the collection already has documents, indicating it exists and is populated
                var count = await shortenedUrlsCollection.CountDocumentsAsync(FilterDefinition<ShortenedUrlString>.Empty);
                return count > 0; // Return true if the collection exists and has documents
            },
            QueryExecution: async () =>
            {
                var shortenedUrlsCollection = _database.GetCollection<ShortenedUrlString>(TableNames.ShortenedUrlPrefixed(_options.Value.TablePrefix));

                // Insert a sample document to ensure the collection is created if it doesn't already exist
                var document = new ShortenedUrl
                {
                    Id = Guid.NewGuid(), // Generate a new GUID
                    OriginalUrl = "http://example.com", // Example original URL
                    ShortUrl = "http://short.ly/abc123", // Example shortened URL
                    Code = "abc123", // Example code
                    CreatedOnUtc = DateTime.UtcNow // Current UTC time
                };

                await shortenedUrlsCollection.InsertOneAsync(document.ToShortenedUrlString());

                return true;
            }
        );

    }

    private void AddMigration(string MigrationName, string? TableNameWithPrefix = null,
        Func<Task<bool>>? QueryCheckBeforeRunExecution = null, Func<Task<bool>>? QueryExecution = null, bool SaveToHistory = true)
    {
        try
        {
            var migrationCollection = _database.GetCollection<BsonDocument>(TableNames.MigrationsPrefixed(_options.Value.TablePrefix));

            // Check if the migration already exists
            var migrationExists = QueryCheckBeforeRunExecution?.Invoke().Result ?? false;

            if (migrationExists)
                return;

            _logger?.LogInformation("Applying migration '{MigrationName}'...", MigrationName);

            // Execute the migration query (MongoDB doesn't require SQL-style queries, so we use Insert, Update, etc.)
            var migrationApplied = QueryExecution?.Invoke().Result ?? false;

            if (migrationApplied && SaveToHistory)
            {
                // Log the applied migration
                var historyDocument = new BsonDocument
                {
                    { "MigrationName", MigrationName },
                    { "AppliedAt", DateTime.UtcNow }
                };
                migrationCollection.InsertOne(historyDocument);
            }

            _logger?.LogInformation("Migration '{MigrationName}' applied successfully.", MigrationName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error applying migration.");
            throw;
        }
    }

    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var migration in ShortenedUrlMigrationTracker.Migrations)
        {
            try
            {
                var exists = await CheckMigrationExistsAsync(migration, cancellationToken);
                if (exists) continue;
                _logger?.LogInformation("Applying migration '{MigrationName}'...", migration.MigrationName);

                using var transaction = await _database.Client.StartSessionAsync(cancellationToken: cancellationToken);
                transaction.StartTransaction();

                try
                {
                    // Execute the migration logic
                    await (migration.QueryExecution?.Invoke() ?? Task.CompletedTask);

                    // Log the migration history
                    if (migration.SaveToHistory)
                    {
                        var migrationCollection = _database.GetCollection<BsonDocument>("migrations");
                        var logDocument = new BsonDocument
                        {
                            { "MigrationName", migration.MigrationName },
                            { "AppliedAt", DateTime.UtcNow }
                        };
                        await migrationCollection.InsertOneAsync(logDocument, cancellationToken: cancellationToken);
                    }

                    await transaction.CommitTransactionAsync(cancellationToken);
                    _logger?.LogInformation("Migration '{MigrationName}' applied successfully.", migration.MigrationName);
                }
                catch (Exception)
                {
                    await transaction.AbortTransactionAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error applying migration.");
                throw;
            }
        }
    }

    private async Task<bool> CheckMigrationExistsAsync(ShortenedUrlMigrationWithQuery migration, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(migration.QueryCheckBeforeRunExecution?.ToString()))
                return true;

            var migrationCollection = _database.GetCollection<BsonDocument>("migrations");
            var filter = Builders<BsonDocument>.Filter.Eq("MigrationName", migration.MigrationName);
            var count = await migrationCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            if (count > 0)
            {
                _logger?.LogWarning("Migration '{MigrationName}' already applied.", migration.MigrationName);
            }

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking migration existence.");
            throw;
        }
    }
}
