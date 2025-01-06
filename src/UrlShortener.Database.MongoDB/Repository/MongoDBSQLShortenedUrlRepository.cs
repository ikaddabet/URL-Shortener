using UrlShortener.Core.Entities;
using UrlShortener.Core.Repository;

namespace UrlShortener.Database.MongoDB.Repository;
public class MongoDBSQLShortenedUrlRepository : IShortenedUrlRepository
{
    public Task<string?> GetAsync(string Code)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(ShortenedUrl record)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsExistsAsync(string Code)
    {
        throw new NotImplementedException();
    }
}
