namespace UrlShortener.Services.UrlShortener;
public interface IUrlShortenerService
{
    /// <summary>
    /// Retrieves the original URL from a shortened code.
    /// </summary>
    /// <param name="Code">The shortened code associated with the original URL.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the original URL.</returns>
    Task<string> GetOriginalUrl(string Code);

    /// <summary>
    /// Creates a shortened URL for the provided original URL.
    /// </summary>
    /// <param name="url">The original URL that needs to be shortened.</param>
    /// <returns>A task representing the asynchronous operation, with a string result containing the shortened URL.</returns>
    Task<string> CreateShortUrl(Uri url);
}
