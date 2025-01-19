using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Repository;
using UrlShortener.Core;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.PostgreSQL.Repository;

public class PostgreSQLShortenedUrlRepository(IOptions<UrlShortenerOptions> options) : IShortenedUrlRepository
{
    private readonly string _connectionString = options.Value.ConnectionString;

    // Get the shortened URL record by Code
    public async Task<string?> GetAsync(string code)
    {
        var query = $@"
                SELECT OriginalUrl FROM {TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)} 
                WHERE Code = @Code LIMIT 1;
        ";
        try
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);

            var result = await connection.QueryFirstOrDefaultAsync<string>(query, new { Code = code });

            return result;
        }
        catch
        {
            throw;
        }
    }

    // Add a new shortened URL record
    public async Task AddAsync(ShortenedUrl record)
    {
        var query = $@"
                INSERT INTO {TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)} (OriginalUrl, Code, ShortUrl)
                VALUES (@OriginalUrl, @Code, @ShortUrl);
        ";

        try
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);

            await connection.ExecuteAsync(query, record);
        }
        catch
        {
            throw;
        }
    }

    // Check if a shortened URL with the given Code already exists
    public async Task<bool> IsExistsAsync(string code)
    {
        var query = $@"
                SELECT COUNT(1) FROM {TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)}
                WHERE Code = @Code;
        ";
        try
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);

            var count = await connection.ExecuteScalarAsync<int>(query, new { Code = code });

            return count > 0;
        }
        catch
        {
            throw;
        }
    }
}

