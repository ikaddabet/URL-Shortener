using Microsoft.Extensions.Options;
using MySqlConnector;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Repository;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.MySQL.Repository;
public class MySQLShortenedUrlRepository(IOptions<UrlShortenerOptions> options) : IShortenedUrlRepository
{

    private readonly string _connectionString = options.Value.ConnectionString;

    // Get the shortened URL record by Code
    public async Task<string?> GetAsync(string Code)
    {
        if (string.IsNullOrEmpty(Code))
            throw new ArgumentException("Code cannot be null or empty", nameof(Code));

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = $@"
        SELECT OriginalUrl
        FROM {TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)}
        WHERE Code = @Code";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Code", Code);

        var result = await command.ExecuteScalarAsync();
        return result as string;
    }

    // Add a new shortened URL record
    public async Task AddAsync(ShortenedUrl record)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = $@"
        INSERT INTO {TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)} (OriginalUrl, ShortUrl, Code, CreatedOnUtc)
        VALUES (@OriginalUrl, @ShortUrl, @Code, @CreatedOnUtc)";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@OriginalUrl", record.OriginalUrl);
        command.Parameters.AddWithValue("@ShortUrl", record.ShortUrl);
        command.Parameters.AddWithValue("@Code", record.Code);
        command.Parameters.AddWithValue("@CreatedOnUtc", record.CreatedOnUtc);

        await command.ExecuteNonQueryAsync();
    }

    // Check if a shortened URL with the given Code already exists
    public async Task<bool> IsExistsAsync(string Code)
    {
        if (string.IsNullOrEmpty(Code))
            throw new ArgumentException("Code cannot be null or empty", nameof(Code));

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = $@"
        SELECT COUNT(1)
        FROM {TableNames.ShortenedUrlPrefixed(options.Value.TablePrefix)}
        WHERE Code = @Code";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Code", Code);

        var count = (long?)await command.ExecuteScalarAsync() ?? 0;
        return count > 0;
    }

}
