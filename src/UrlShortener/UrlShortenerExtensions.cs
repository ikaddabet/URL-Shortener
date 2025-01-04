using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using UrlShortener.Core.Models;
using UrlShortener.Services.UrlShortening;


namespace UrlShortener;

public static class UrlShortenerExtensions
{
    public static void AddUrlShortener([NotNull] this IServiceCollection services)
    {
        // Todo: Add HttpContextAccessor Dynamic

        //if (!services.Any(service => service.ServiceType == typeof(IHttpContextAccessor)))
        //{
        //    services.AddHttpContextAccessor();
        //}

        services.AddScoped<IUrlShorteningService, UrlShorteningService>();
    }

    public static void UseUrlShortener([NotNull] this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/shorten", async (ShortenUrlRequest request, IUrlShorteningService urlShorteningService) =>
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
            {
                return Results.BadRequest("The specified URL is invalid.");
            }

            var shortUrl = await urlShorteningService.CreateShortUrl(uri);

            if (string.IsNullOrEmpty(shortUrl))
                return Results.BadRequest("Failed to create short URL.");

            return Results.Ok(shortUrl);
        });

        endpoints.MapGet("api/{code}", async (string code, IUrlShorteningService urlShorteningService) =>
        {
            string longUrl = await urlShorteningService.GetLongUrl(code);

            if (string.IsNullOrEmpty(longUrl))
                return Results.NotFound();

            return Results.Redirect(longUrl);
        });
    }
}

