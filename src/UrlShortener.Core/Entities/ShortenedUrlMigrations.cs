namespace UrlShortener.Core.Entities;
public class ShortenedUrlMigration
{
    public required string MigrationName { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
public class ShortenedUrlMigrationWithQuery : ShortenedUrlMigration
{
    public string? TableNameWithPrefix { get; set; }
    public string? QueryCheckBeforeExectution { get; set; }
    public string? Query { get; set; }

    public ShortenedUrlMigration ToMigration() => new()
    {
        MigrationName = MigrationName,
        AppliedAt = AppliedAt
    };
}

