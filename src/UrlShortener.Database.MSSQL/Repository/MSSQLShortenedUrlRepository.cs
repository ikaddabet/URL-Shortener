using UrlShortener.Core.Entities;
using UrlShortener.Core.Repository;

namespace UrlShortener.Database.MSSQL.Repository;
public class MSSQLShortenedUrlRepository : IShortenedUrlRepository
{
    public Task AddAsync(ShortenedUrl record)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetAsync(string Code)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsExistsAsync(string Code)
    {
        throw new NotImplementedException();
    }
}
