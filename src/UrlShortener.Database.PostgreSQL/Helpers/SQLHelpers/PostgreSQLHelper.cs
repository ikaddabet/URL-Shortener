using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostgreSQLConnector;
using System.Text.RegularExpressions;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Helpers.SQLHelpers;
using UrlShortener.Core.Migrations;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.PostgreSQL.Helpers.SQLHelpers;

public partial class PostgreSQLHelper(ILogger<PostgreSQLHelper> logger, IOptions<UrlShortenerOptions> options) : ISQLHelper
{
    [GeneratedRegex(@"^[a-zA-Z0-9_-]{1,64}$")]
    private static partial Regex DatabaseNameRegex();

    public void CheckDatabaseName()
    {
    }

    public async Task CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
    }

    public void AddMigrations()
    {
        
    }

    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {

    }

    private async Task<bool> CheckMigartionExistsAsync(ShortenedUrlMigrationWithQuery Migration, CancellationToken cancellationToken = default)
    {

    }

}
