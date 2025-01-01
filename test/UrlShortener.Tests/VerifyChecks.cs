using UrlShortener.Helper;
using FluentAssertions;
using UrlShortener.Models;

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
        randomCode.Length.Should().Be(ShortLinkSettings.Length, $"The random code should have a length of {ShortLinkSettings.Length} characters.");

        await Task.CompletedTask;
    }
}
