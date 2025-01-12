using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using UrlShortener.Core;
using UrlShortener.Core.Helpers.SQLHelpers;
using UrlShortener.Core.Helpers.UrlShorteningHelper;
using UrlShortener.Core.Models;
using UrlShortener.Core.Repository;
using UrlShortener.Database.MSSQL.Helpers.SQLHelpers;
using UrlShortener.Database.MSSQL.Repository;
using UrlShortener.DatabaseInitializer;
using UrlShortener.Services.DatabaseBackground;
using UrlShortener.Services.UrlShortener;

namespace UrlShortener;

public static class UrlShortenerExtensions
{
    public static UrlShortenerBuilderOptions AddUrlShortener(this IServiceCollection services, Action<UrlShortenerOptions>? configureOptions = null)
    {
        var options = new UrlShortenerOptions();
        configureOptions?.Invoke(options);

        if (!services.Any(service => service.ServiceType == typeof(IHttpContextAccessor)))
        {
            services.AddHttpContextAccessor();
        }

        services.AddSingleton<IOptions<UrlShortenerOptions>>(new OptionsWrapper<UrlShortenerOptions>(options));
        services.AddSingleton<UrlShorteningHelper>();
        services.AddScoped<IUrlShortenerService, UrlShortenerService>();

        return new UrlShortenerBuilderOptions(services);
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

public class UrlShortenerBuilderOptions(IServiceCollection services) : UrlShortenerBuilder(services)
{
    private readonly IServiceCollection _services = services;

    public UrlShortenerBuilder AddConfiguration(IConfiguration configuration)
    {
        ServiceDescriptor? isConfigured = _services.FirstOrDefault(service => service.ServiceType == typeof(IOptions<UrlShortenerOptions>));
        if (isConfigured is not null)
        {
            bool isRemoved = _services.Remove(isConfigured);
            if (isRemoved)
            {
                _services.Configure<UrlShortenerOptions>(configuration.GetSection("UrlShortenerOptions"));
            }
        }

        return new UrlShortenerBuilder(_services);
    }
}

public class UrlShortenerBuilder(IServiceCollection services)
{
    //public void AddMongoDB()
    //{
    //    services.AddSingleton<ISQLHelper, MongoDBSQLHelper>();
    //    services.AddScoped<IUrlShortenerDatabaseInitializer, UrlShortenerDatabaseInitializer>();
    //    services.AddScoped<IShortenedUrlRepository, MongoDBSQLShortenedUrlRepository>();
    //    services.AddHostedService<DatabaseBackgroundService>();
    //}
    public void AddMSSQL()
    {
        services.AddSingleton<ISQLHelper, MSSQLHelper>();
        services.AddScoped<IShortenedUrlRepository, MSSQLShortenedUrlRepository>();
        services.AddSingleton<IUrlShortenerDatabaseInitializer, UrlShortenerDatabaseInitializer>();
        services.AddHostedService<DatabaseBackgroundService>();
    }
    //public void AddMySQL()
    //{
    //    services.AddSingleton<ISQLHelper, MySQLHelper>();
    //    services.AddScoped<IUrlShortenerDatabaseInitializer, UrlShortenerDatabaseInitializer>();
    //    services.AddScoped<IShortenedUrlRepository, MySQLShortenedUrlRepository>();
    //    services.AddHostedService<DatabaseBackgroundService>();
    //}
}
