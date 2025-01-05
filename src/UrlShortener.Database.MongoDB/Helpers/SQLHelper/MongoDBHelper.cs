﻿using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using UrlShortener.Core;
using UrlShortener.Core.Helpers.SQLHelper;

namespace UrlShortener.Database.MongoDB.Helpers.SQLHelper;

public class MongoDBSQLHelper : ISQLHelper
{
    private readonly ILogger<MongoDBSQLHelper> _logger;
    private readonly UrlShortenerOptions _options;
    private readonly IMongoDatabase _database;

    public MongoDBSQLHelper(ILogger<MongoDBSQLHelper> logger, UrlShortenerOptions options)
    {
        _logger = logger;
        _options = options;

        var client = new MongoClient(options.ConnectionString);
        client.GetDatabase(options.DatabaseName);
        _database = client.GetDatabase(options.DatabaseName);
    }

    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.RunCommandAsync((Command<BsonDocument>)"{ ping: 1 }", cancellationToken: cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MongoDB.");
            return false;
        }
    }

    public async Task<bool> CheckTableExistsAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        try
        {
            string collectionNameWithPrefix = _options.TablePrefix + collectionName;

            var collections = await _database.ListCollectionNamesAsync(cancellationToken: cancellationToken);
            var collectionList = await collections.ToListAsync(cancellationToken);

            return collectionList.Contains(collectionNameWithPrefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if the collection exists.");
            return false;
        }
    }

    public async Task<bool> CreateTableAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        try
        {
            string collectionNameWithPrefix = _options.TablePrefix + collectionName;

            // Check if the collection already exists
            bool exists = await CheckTableExistsAsync(collectionNameWithPrefix, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Collection '{collectionName}' already exists.", collectionNameWithPrefix);
                return false;
            }

            // Create the collection if it doesn't exist
            await _database.CreateCollectionAsync(collectionNameWithPrefix, cancellationToken: cancellationToken);
            _logger.LogInformation("Collection '{collectionName}' created successfully.", collectionNameWithPrefix);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create collection.");
            return false;
        }
    }

    public async Task<string> GetDatabaseVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var serverStatus = await _database.RunCommandAsync((Command<BsonDocument>)"{ buildInfo: 1 }", cancellationToken: cancellationToken);
            return serverStatus["version"]?.ToString() ?? throw new Exception("Failed to get database version.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MongoDB version.");
            return string.Empty;
        }
    }
}
