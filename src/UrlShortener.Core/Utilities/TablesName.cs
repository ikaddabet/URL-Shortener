namespace UrlShortener.Core.Utilities;
public static class TableNames
{
    public static string ShortenedUrl => "Shortened";
    public static string ShortenedUrlPrefixed(string Prefix) => $"{Prefix}_{ShortenedUrl}".ToLower(); // Some databases are case sensitive
    public static string Migrations => "Migrations";
    public static string MigrationsPrefixed(string Prefix) => $"{Prefix}__{Migrations}".ToLower(); // Some databases are case sensitive
}
