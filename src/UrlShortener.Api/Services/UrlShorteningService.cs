using Microsoft.EntityFrameworkCore;
using UrlShortener.Helper;
using UrlShortener.Models;

namespace UrlShortener.Api.Services;

public class UrlShorteningService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<string> GenerateUniqueCode()
    {
        while (true) {
            var Code = UrlShorteningHelper.GenerateShortLink();
            if (! await _context.ShortenedUrls.AnyAsync(x => x.Code == Code))
            {
                return Code;
            }
        }
    }
}
