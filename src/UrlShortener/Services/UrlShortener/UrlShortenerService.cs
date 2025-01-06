using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UrlShortener.Core;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Helpers.UrlShorteningHelper;
using UrlShortener.Core.Repository;

namespace UrlShortener.Services.UrlShortener;

internal class UrlShortenerService(IShortenedUrlRepository shortenedUrlRepository, IHttpContextAccessor httpContext, ILogger<UrlShortenerService> logger, UrlShorteningHelper urlShorteningHelper) : IUrlShortenerService
{
    private readonly HttpContext _httpContext = httpContext.HttpContext ?? throw new ArgumentNullException("""
        IHttpContextAccessor is null. 
        Please ensure that IHttpContextAccessor is registered in the dependency injection container. 
        To resolve this, add the following to your Program.cs or Startup.cs file:

        builder.Services.AddHttpContextAccessor();

        This will ensure that the HttpContext is properly injected into the service.
        """);
    private const int MaxRetries = 3;

    public async Task<string> GetOriginalUrl(string Code)
    {
        if (string.IsNullOrWhiteSpace(Code))
            return string.Empty;

        try
        {
            return await shortenedUrlRepository.GetAsync(Code) ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve the long URL.");
            return string.Empty;
        }
    }

    public async Task<string> CreateShortUrl(Uri url)
    {
        try
        {
            var Code = await GenerateUniqueCode();

            var shortenedUrl = new ShortenedUrl
            {
                Id = Guid.NewGuid(),
                LongUrl = url.ToString(),
                Code = Code,
                ShortUrl = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}/api/{Code}",
                CreatedOnUtc = DateTime.UtcNow
            };

            await shortenedUrlRepository.AddAsync(shortenedUrl);

            return shortenedUrl.ShortUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the short URL.");
            return string.Empty;
        }
    }

    private async Task<string> GenerateUniqueCode()
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var Code = urlShorteningHelper.GenerateRandomCode();
                if (await shortenedUrlRepository.IsExistsAsync(Code))
                {
                    throw new InvalidOperationException("Short code already exists.");
                }
                return Code;
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetries)
                {
                    logger.LogError("Failed to generate a unique code after {MaxRetries} retries. Aborting.", MaxRetries);

                    throw new InvalidOperationException("Failed to generate a unique short code.", ex);
                }

                logger.LogError("Short code already exists. Retrying... (Attempt {Attempt} of {MaxRetries})", attempt + 1, MaxRetries);
            }
        }
        throw new InvalidOperationException("Failed to generate a unique short code.");
    }
}
