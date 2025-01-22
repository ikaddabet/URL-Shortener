namespace UrlShortener.Core.Entities;

public class ShortenedUrlBase<TId>
{
    public TId Id { get; set; } = default!;

    public string OriginalUrl { get; set; } = string.Empty;

    public string ShortUrl { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; }
}

public class ShortenedUrl : ShortenedUrlBase<Guid>
{
    public ShortenedUrlString ToShortenedUrlString() => new() { Id = Id.ToString(), OriginalUrl = OriginalUrl, ShortUrl = ShortUrl, Code = Code, CreatedOnUtc = CreatedOnUtc };
}

public class ShortenedUrlString : ShortenedUrlBase<String>
{
    public ShortenedUrl ToShortenedUrlGuid() => new() { Id = new Guid(Id), OriginalUrl = OriginalUrl, ShortUrl = ShortUrl, Code = Code, CreatedOnUtc = CreatedOnUtc };

}
