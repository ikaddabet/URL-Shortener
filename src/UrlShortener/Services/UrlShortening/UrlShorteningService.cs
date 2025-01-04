using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortener.Core.Helper;
using UrlShortener.Data;
using UrlShortener.Entities;

namespace UrlShortener.Services.UrlShortening;

internal class UrlShorteningService(ApplicationDbContext context, IHttpContextAccessor httpContext, ILogger<UrlShorteningService> logger) : IUrlShorteningService
{
    private readonly ApplicationDbContext _context = context;
    private readonly HttpContext _httpContext = httpContext.HttpContext ?? throw new ArgumentNullException("""
        IHttpContextAccessor is null. 
        Please ensure that IHttpContextAccessor is registered in the dependency injection container. 
        To resolve this, add the following to your Program.cs or Startup.cs file:

        builder.Services.AddHttpContextAccessor();

        This will ensure that the HttpContext is properly injected into the service.
        """);
    private readonly ILogger<UrlShorteningService> _logger = logger;

    public async Task<string> GetLongUrl(string Code)
    {
        if (string.IsNullOrWhiteSpace(Code))
            return string.Empty;

        try
        {
            return await _context.ShortenedUrls.Select(x => new { x.Code, x.LongUrl })
                                               .Where(x => x.Code == Code)
                                               .Select(x => x.LongUrl)
                                               .FirstOrDefaultAsync() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve the long URL.");
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

            _context.ShortenedUrls.Add(shortenedUrl);

            await _context.SaveChangesAsync();

            return shortenedUrl.ShortUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the short URL.");
            return string.Empty;
        }
    }

    private async Task<string> GenerateUniqueCode()
    {
        while (true)
        {
            var Code = UrlShorteningHelper.GenerateRandomCode();
            if (!await _context.ShortenedUrls.AnyAsync(x => x.Code == Code))
            {
                return Code;
            }
        }
    }
}
