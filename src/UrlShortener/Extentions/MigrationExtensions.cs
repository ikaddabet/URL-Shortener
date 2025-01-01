using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using UrlShortener.Data;

namespace UrlShortener.Extentions;

internal static class MigrationExtensions
{
    public static void ApplyMigrations([NotNull] this WebApplication app)
    {
        if (app.Environment.IsDevelopment() && false)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Database.Migrate();
        }
    }
}
