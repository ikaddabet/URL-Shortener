using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Core.Entities;
public class ShortenedUrlMigration
{
    public required string MigrationName { get; set; }
    public DateTime AppliedAt { get; set; }
}
public class ShortenedUrlMigrationWithQuery : ShortenedUrlMigration
{
    public required string TableName { get; set; }
    public required string TableNameWithPrefix { get; set; }
    public required string CreateTableQuery { get; set; }

    public ShortenedUrlMigration ToMigration() => new()
    {
        MigrationName = MigrationName,
        AppliedAt = AppliedAt
    };
}

