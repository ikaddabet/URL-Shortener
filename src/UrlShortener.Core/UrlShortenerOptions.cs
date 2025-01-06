namespace UrlShortener.Core;

public class UrlShortenerOptions
{
    /// <summary>
    /// Default length of the shortened URL path.
    /// </summary>
    protected const int DefaultLength = 7;

    /// <summary>
    /// Default character set used to generate shortened URLs.
    /// </summary>
    protected const string DefaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// Default connection string used to connect to the database.
    /// </summary>
    protected const string DefaultTablePrefix = "_urlShortener_";

    /// <summary>
    /// The length of the shortened URL path.
    /// </summary>
    public int Length { get; set; } = DefaultLength;

    /// <summary>
    /// The character set used to generate shortened URLs.
    /// </summary>
    public string Alphabet { get; set; } = DefaultAlphabet;

    /// <summary>
    /// The table prefix used to create the database tables.
    /// </summary>
    public string TablePrefix { get; set; } = DefaultTablePrefix;

    /// <summary>
    /// The connection string used to connect to the database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The name of the database to use.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;
}


