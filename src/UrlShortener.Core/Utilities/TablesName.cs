namespace UrlShortener.Core.Utilities;
public static class TableNames
{
    public static string ShortenedUrl => "Shortened";
    public static string ShortenedUrlPrefixed(string Prefix) => $"{Prefix}_{ShortenedUrl}";
    public static string Migrations => "Migrations";
    public static string MigrationsPrefixed(string Prefix) => $"{Prefix}__{Migrations}";
}
