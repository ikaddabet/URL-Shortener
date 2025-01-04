﻿using Microsoft.EntityFrameworkCore;
using UrlShortener.Core;
using UrlShortener.Core.Entities;

namespace UrlShortener.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortenedUrl>(builder =>
        {
            builder.Property(x => x.Code).HasMaxLength(UrlShortenerSettings.Length);

            builder.HasIndex(x => x.Code).IsUnique();
        });
    }
}
