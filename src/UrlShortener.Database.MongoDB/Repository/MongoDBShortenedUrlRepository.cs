using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Repository;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.MongoDB.Repository;

public class MongoDBShortenedUrlRepository : IShortenedUrlRepository
{
    private readonly IMongoCollection<ShortenedUrlString> _shortenedUrls;

    public MongoDBShortenedUrlRepository(IOptions<UrlShortenerOptions> options)
    {
        // Connect to MongoDB using connection string from options
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        _shortenedUrls = database.GetCollection<ShortenedUrlString>(TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix));
    }

    // Get the shortened URL record by Code
    public async Task<string?> GetAsync(string code)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Code cannot be null or empty", nameof(code));

        var filter = Builders<ShortenedUrlString>.Filter.Eq(u => u.Code, code);
        var result = await _shortenedUrls.Find(filter).FirstOrDefaultAsync();
        return result?.OriginalUrl;
    }

    // Add a new shortened URL record
    public async Task AddAsync(ShortenedUrl record)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        await _shortenedUrls.InsertOneAsync(record.ToShortenedUrlString());
    }

    // Check if a shortened URL with the given Code already exists
    public async Task<bool> IsExistsAsync(string code)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Code cannot be null or empty", nameof(code));

        var filter = Builders<ShortenedUrlString>.Filter.Eq(u => u.Code, code);
        var count = await _shortenedUrls.CountDocumentsAsync(filter);
        return count > 0;
    }
}
