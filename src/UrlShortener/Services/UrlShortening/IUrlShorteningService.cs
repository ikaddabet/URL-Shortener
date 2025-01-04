namespace UrlShortener.Services.UrlShortening;
public interface IUrlShorteningService
{
    Task<string> GetLongUrl(string Code);
    Task<string> CreateShortUrl(Uri url);
}
