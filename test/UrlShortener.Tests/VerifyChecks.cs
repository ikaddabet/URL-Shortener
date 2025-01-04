using FluentAssertions;
using UrlShortener.Core;
using UrlShortener.Core.Helper;

namespace UrlShortener.Tests;

internal static class VerifyChecks
{
    public static async Task CheckGenerateRandomCode()
    {
        // Generate the random code
        var randomCode = UrlShorteningHelper.GenerateShortLink();

        // Ensure that the generated code is not null or empty
        randomCode.Should().NotBeNullOrEmpty("The random code should not be null or empty.");

        // Check if the code follows an expected pattern or length.
        randomCode.Length.Should().Be(UrlShortenerSettings.Length, $"The random code should have a length of {UrlShortenerSettings.Length} characters.");

        await Task.CompletedTask;
    }
}
