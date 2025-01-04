namespace UrlShortener.Core.Helper;

public static class UrlShorteningHelper
{
    private static readonly Random _random = new();

    /// <summary>
    /// Generates a random code
    /// </summary>
    /// <returns>Random code Passed on <see cref="UrlShortenerSettings"/> </returns>
    public static string GenerateRandomCode()
    {
        return new string(Enumerable.Repeat(UrlShortenerSettings.Alphabet, UrlShortenerSettings.Length).Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
