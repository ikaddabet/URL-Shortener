using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace UrlShortener.Api.Extentions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }
}
