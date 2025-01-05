using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Repository;
public interface IShortenedUrlRepository
{
    Task<string?> GetAsync(string Code);
    Task AddAsync(ShortenedUrl record);
    Task<bool> IsExistsAsync(string Code);
}
