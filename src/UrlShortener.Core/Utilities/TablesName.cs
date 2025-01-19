namespace UrlShortener.Core.Utilities;
public static class TableNames
{
    public static string ShortenedUrl => "Shortened";
    /// <summary>
    /// Generates a table name for storing shortened URLs, prefixed by a provided prefix.
    /// The result is converted to lower case with an underscore separating the prefix and the table name.
    /// This method ensures the table name adheres to naming conventions for case-sensitive databases.
    /// </summary>
    /// <param name="Prefix">The prefix to add to the table name.</param>
    /// <returns>The prefixed table name in lowercase format.</returns>
    public static string ShortenedUrlPrefixed(string Prefix) => $"{Prefix}_{ShortenedUrl}".ToLower(); // Some databases are case sensitive
    public static string Migrations => "Migrations";
    /// <summary>
    /// Generates a table name for storing migration data, prefixed by a provided prefix.
    /// The result is converted to lower case with double underscores separating the prefix and the table name.
    /// This method ensures the table name adheres to naming conventions for case-sensitive databases.
    /// </summary>
    /// <param name="Prefix">The prefix to add to the table name.</param>
    /// <returns>The prefixed table name in lowercase format with double underscores.</returns>
    public static string MigrationsPrefixed(string Prefix) => $"{Prefix}__{Migrations}".ToLower(); // Some databases are case sensitive
}
