using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using UrlShortener.Core;
using UrlShortener.Core.Helpers.UrlShorteningHelper;
using UrlShortener.Core.Models;
using UrlShortener.Services.UrlShortener;

namespace UrlShortener;

public static class UrlShortenerExtensions
{
    /// <summary>
    /// Adds and configures the URL shortener services to the dependency injection container, enabling 
    /// the functionality to shorten URLs within the application. This method provides a way to integrate 
    /// the URL shortener service into an ASP.NET Core application by registering essential components 
    /// and options required for its operation.
    ///
    /// The main purpose of this method is to set up the necessary services, such as configuration options, 
    /// HTTP context access, and URL shortening logic, ensuring that the application has all the required 
    /// dependencies to shorten and manage URLs.
    ///
    /// Additionally, this method supports an optional <paramref name="configureOptions"/> parameter, which 
    /// allows the consumer of this API to customize the behavior of the URL shortener by configuring the 
    /// <see cref="UrlShortenerOptions"/> before they are applied.
    ///
    /// The core components registered by this method include:
    /// <list type="bullet">
    ///   <item><description><see cref="IHttpContextAccessor"/> - Provides access to the current HTTP context, 
    ///   essential for certain URL shortening operations that depend on the request context.</description></item>
    ///   <item><description><see cref="IOptions{UrlShortenerOptions}"/> - Holds the configuration options 
    ///   for the URL shortener, allowing the system to be customized according to the application's needs.</description></item>
    ///   <item><description><see cref="UrlShorteningHelper"/> - A utility class that encapsulates the logic 
    ///   for shortening URLs efficiently and securely.</description></item>
    ///   <item><description><see cref="IUrlShortenerService"/> - The main service interface for performing URL 
    ///   shortening operations.</description></item>
    /// </list>
    /// 
    /// This method is designed to be used as part of the initial setup for an application that needs to 
    /// provide URL shortening capabilities, abstracting the underlying complexity of dependency registration 
    /// and configuration management.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the URL shortener services will be added.</param>
    /// <param name="configureOptions">An optional delegate to configure <see cref="UrlShortenerOptions"/>.</param>
    /// <returns>Returns an instance of <see cref="UrlShortenerBuilderOptions"/> to allow further configuration and chaining.</returns>

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

    /// <summary>
    /// Registers the URL shortener endpoints to the specified route builder, allowing clients to create 
    /// short URLs and resolve them to their original long URLs. These endpoints provide the core functionality 
    /// for interacting with the URL shortening service via HTTP requests.
    ///
    /// The following endpoints are added:
    /// <list type="bullet">
    ///   <item><description><b>POST api/shorten</b> - Accepts a long URL in the request body and returns a shortened version of the URL.
    ///     The request must contain a valid URL. If the URL is invalid or if the shortening fails, a bad request response is returned.</description></item>
    ///   <item><description><b>GET api/{code}</b> - Redirects to the original URL corresponding to the provided shortened code.
    ///     If the code is valid and the original URL exists, the client is redirected to the long URL. If the code is invalid or not found, 
    ///     a "not found" response is returned.</description></item>
    /// </list>
    /// 
    /// These endpoints are designed to handle typical URL shortening operations, with error handling for invalid or unsuccessful requests.
    ///
    /// <para>
    /// This method is intended to be called within the `Configure` method of your application's startup class to register the routing logic
    /// for the URL shortener feature. Additionally, the endpoints are automatically documented in Swagger if the application has Swagger 
    /// set up, thanks to the use of attribute-based documentation and standard HTTP status codes.
    /// </para>
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to which the URL shortener routes will be added.</param>
    /// <remarks>
    /// The <b>POST api/shorten</b> endpoint requires a valid JSON body with a "Url" field, while the <b>GET api/{code}</b> endpoint expects 
    /// the shortened URL code to be provided in the route.
    /// </remarks>
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
            var OriginalUrl = await urlShorteningService.GetOriginalUrl(code);

            if (string.IsNullOrEmpty(OriginalUrl))
            {
                return Results.NotFound();
            }

            return Results.Redirect(OriginalUrl);
        });
    }

}