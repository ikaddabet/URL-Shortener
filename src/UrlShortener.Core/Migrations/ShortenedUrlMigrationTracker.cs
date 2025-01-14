﻿using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Migrations;
public static class ShortenedUrlMigrationTracker
{
    public static List<ShortenedUrlMigrationWithQuery> Migrations = [];

    public static void AddMigration(string MigrationName, string? TableNameWithPrefix = null, string? QueryCheckBeforeExecution = null, string? Query = null)
    {
        // Trim leading and trailing spaces from the MigrationName
        string trimmedMigrationName = MigrationName.Trim();

        // Remove any remaining white spaces from the middle of the string
        string cleanedMigrationName = trimmedMigrationName.Replace(" ", "-").ToLower();

        // Ensure the name is unique by appending a timestamp
        string uniqueMigrationName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{cleanedMigrationName}";

        // Add the migration to the collection with the unique name
        Migrations.Add(new ShortenedUrlMigrationWithQuery
        {
            MigrationName = uniqueMigrationName,
            TableNameWithPrefix = TableNameWithPrefix,
            QueryCheckBeforeExecution = QueryCheckBeforeExecution,
            Query = Query,
        });

        Console.WriteLine($"Migration '{uniqueMigrationName}' added.");
    }

}
