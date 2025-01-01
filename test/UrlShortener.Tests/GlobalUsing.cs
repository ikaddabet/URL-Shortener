namespace UrlShortener.Tests;

public class GlobalUsing
{
    [Fact]
    public async Task Run()
    {
        await VerifyChecks.CheckGenerateRandomCode();
    }
}
