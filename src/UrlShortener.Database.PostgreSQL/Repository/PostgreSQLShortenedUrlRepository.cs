using Microsoft.Extensions.Options;
using PostgreSQLConnector;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Repository;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.PostgreSQL.Repository;
public class PostgreSQLShortenedUrlRepository(IOptions<UrlShortenerOptions> options) : IShortenedUrlRepository
{

    private readonly string _connectionString = options.Value.ConnectionString;

    // Get the shortened URL record by Code
    public async Task<string?> GetAsync(string Code)
    {

    }

    // Add a new shortened URL record
    public async Task AddAsync(ShortenedUrl record)
    {

    }

    // Check if a shortened URL with the given Code already exists
    public async Task<bool> IsExistsAsync(string Code)
    {

    }

}
