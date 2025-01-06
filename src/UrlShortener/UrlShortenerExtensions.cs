using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using UrlShortener.Core;
using UrlShortener.Core.DatabaseInitializer;
using UrlShortener.Core.Helpers.SQLHelpers;
using UrlShortener.Core.Helpers.UrlShorteningHelper;
using UrlShortener.Core.Models;
using UrlShortener.Core.Repository;
using UrlShortener.Database.MongoDB.Helpers.SQLHelpers;
using UrlShortener.Database.MongoDB.Repository;
using UrlShortener.Database.MSSQL.Helpers.SQLHelpers;
using UrlShortener.Database.MSSQL.Repository;
using UrlShortener.Database.MySQL.Helpers.SQLHelpers;
using UrlShortener.Database.MySQL.Repository;
using UrlShortener.Services.DatabaseBackground;
using UrlShortener.Services.UrlShortener;

namespace UrlShortener;

public static class UrlShortenerExtensions
{
    public static UrlShortenerBuilder AddUrlShortener(this IServiceCollection services, Action<UrlShortenerOptions>? configureOptions = null)
    {
        var options = new UrlShortenerOptions();
        configureOptions?.Invoke(options);

        if (!services.Any(service => service.ServiceType == typeof(IHttpContextAccessor)))
        {
            services.AddHttpContextAccessor();
        }

        services.AddSingleton(options);
        services.AddSingleton<UrlShorteningHelper>();
        services.AddScoped<IUrlShortenerService, UrlShortenerService>();

        return new UrlShortenerBuilder(services);
    }

    public static void UseUrlShortener([NotNull] this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/shorten", async (ShortenUrlRequest request, IUrlShortenerService urlShorteningService) =>
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
            {
                return Results.BadRequest("The specified URL is invalid.");
            }

            var shortUrl = await urlShorteningService.CreateShortUrl(uri);

            if (string.IsNullOrEmpty(shortUrl))
            {
                return Results.BadRequest("Failed to create short URL.");
            }

            return Results.Ok(shortUrl);
        });

        endpoints.MapGet("api/{code}", async (string code, IUrlShortenerService urlShorteningService) =>
        {
            var longUrl = await urlShorteningService.GetOriginalUrl(code);

            if (string.IsNullOrEmpty(longUrl))
            {
                return Results.NotFound();
            }

            return Results.Redirect(longUrl);
        });
    }
}

public class UrlShortenerBuilder(IServiceCollection services)
{
    public void AddMongoDB()
    {
        services.AddSingleton<ISQLHelper, MongoDBSQLHelper>();
        services.AddScoped<IUrlShortenerDatabaseInitializer, MongoDBSQLUrlShortenerDatabaseInitializer>();
        services.AddScoped<IShortenedUrlRepository, MongoDBSQLShortenedUrlRepository>();
        services.AddHostedService<DatabaseBackgroundService>();
    }
    public void AddMSSQL()
    {
        services.AddSingleton<ISQLHelper, MSSQLHelper>();
        services.AddScoped<IUrlShortenerDatabaseInitializer, MSSQLUrlShortenerDatabaseInitializer>();
        services.AddScoped<IShortenedUrlRepository, MSSQLShortenedUrlRepository>();
        services.AddHostedService<DatabaseBackgroundService>();
    }
    public void AddMySQL()
    {
        services.AddSingleton<ISQLHelper, MySQLHelper>();
        services.AddScoped<IUrlShortenerDatabaseInitializer, MySQLUrlShortenerDatabaseInitializer>();
        services.AddScoped<IShortenedUrlRepository, MySQLShortenedUrlRepository>();
        services.AddHostedService<DatabaseBackgroundService>();
    }
}
