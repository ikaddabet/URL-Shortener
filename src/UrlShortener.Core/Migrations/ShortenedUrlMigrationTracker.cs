using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Migrations;
public static class ShortenedUrlMigrationTracker
{
    public static List<ShortenedUrlMigrationWithQuery> Migrations = [];

    public static void AddMigration(string MigrationName, string? TableNameWithPrefix = null, string? QueryCheckBeforeRun = null, string? Query = null, bool SaveToHistory = true)
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
            QueryCheckBeforeRun = QueryCheckBeforeRun,
            Query = Query,
            SaveToHistory = SaveToHistory
        });

        Console.WriteLine($"Migration '{uniqueMigrationName}' added.");
    }

    public static void AddMigration(string MigrationName, string? TableNameWithPrefix = null, Func<Task<bool>>? QueryCheckBeforeRunExecution = null, Func<Task<bool>>? QueryExecution = null, bool SaveToHistory = true)
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
            QueryCheckBeforeRunExecution = QueryCheckBeforeRunExecution,
            QueryExecution = QueryExecution,
            SaveToHistory = SaveToHistory
        });

        Console.WriteLine($"Migration '{uniqueMigrationName}' added.");
    }

}
