using UrlShortener.Models;

namespace UrlShortener.Helper;

public static class UrlShorteningHelper
{
    private static readonly Random _random = new();

    public static string GenerateShortLink()
    {
        return new string(Enumerable.Repeat(ShortLinkSettings.Alphabet, ShortLinkSettings.Length).Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
