namespace UrlShortener.Core.Entities;
public class ShortenedUrlMigration
{
    public required string MigrationName { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
public class ShortenedUrlMigrationWithQuery : ShortenedUrlMigration
{
    public string? TableNameWithPrefix { get; set; }
    public string? QueryCheckBeforeRun { get; set; }
    public string? Query { get; set; }
    public Func<Task<bool>>? QueryCheckBeforeRunExecution { get; set; }
    public Func<Task<bool>>? QueryExecution { get; set; }

    public ShortenedUrlMigration ToMigration() => new()
    {
        MigrationName = MigrationName,
        AppliedAt = AppliedAt
    };
}

