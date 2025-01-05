namespace UrlShortener.Core.Helpers.UrlShorteningHelper;

public class UrlShorteningHelper(UrlShortenerOptions UrlShortenerOptions)
{
    private static readonly Random _random = new();

    /// <summary>
    /// Generates a random code
    /// </summary>
    /// <returns>Random code Passed on <see cref="UrlShortenerOptions"/> </returns>
    public string GenerateRandomCode()
    {
        return new string(Enumerable.Repeat(UrlShortenerOptions.Alphabet, UrlShortenerOptions.Length).Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
