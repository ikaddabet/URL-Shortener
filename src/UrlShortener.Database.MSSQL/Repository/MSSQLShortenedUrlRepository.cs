﻿using Microsoft.Data.SqlClient;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Repository;
using UrlShortener.Core.Utilities;

namespace UrlShortener.Database.MSSQL.Repository;
public class MSSQLShortenedUrlRepository(UrlShortenerOptions options) : IShortenedUrlRepository
{
    private readonly string _connectionString = options.ConnectionString;

    // Get the shortened URL record by Code
    public async Task<string?> GetAsync(string Code)
    {
        if (string.IsNullOrEmpty(Code))
            throw new ArgumentException("Code cannot be null or empty", nameof(Code));

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = $@"
            SELECT LongUrl
            FROM {TableNames.ShortenedUrlPrefixed(options.TablePrefix)}
            WHERE Code = @Code";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Code", Code);

        var result = await command.ExecuteScalarAsync();
        return result as string;
    }

    // Add a new shortened URL record
    public async Task AddAsync(ShortenedUrl record)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = $@"
            INSERT INTO {TableNames.ShortenedUrlPrefixed(options.TablePrefix)} (LongUrl, ShortUrl, Code, CreatedOnUtc)
            VALUES (@LongUrl, @ShortUrl, @Code, @CreatedOnUtc)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@LongUrl", record.LongUrl);
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

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = $@"
            SELECT COUNT(1)
            FROM {TableNames.ShortenedUrlPrefixed(options.TablePrefix)}
            WHERE Code = @Code";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Code", Code);

        var count = (int?)await command.ExecuteScalarAsync() ?? 0;
        return count > 0;
    }
}