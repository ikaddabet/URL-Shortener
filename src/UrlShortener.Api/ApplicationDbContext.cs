using Microsoft.EntityFrameworkCore;
using UrlShortener.Entity;
using UrlShortener.Models;

namespace UrlShortener.Api;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortenedUrl>(builder =>
        {
            builder.Property(x => x.Code).HasMaxLength(ShortLinkSettings.Length);

            builder.HasIndex(x => x.Code).IsUnique();
        });
    } 
}
