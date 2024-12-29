using UrlShortener.Models;

namespace UrlShortener.Services;

public class UrlShorteningService
{
    private readonly Random _random = new();

    public string GenerateShortLink()
    {
        return new string(Enumerable.Repeat(ShortLinkSettings.Alphabet, ShortLinkSettings.Length).Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
