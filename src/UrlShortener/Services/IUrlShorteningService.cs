namespace UrlShortener.Services;
public interface IUrlShorteningService
{
    Task<string> GetLongUrl(string Code);
    Task<string> CreateShortUrl(Uri url);
}
