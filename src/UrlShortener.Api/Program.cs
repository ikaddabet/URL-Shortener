using UrlShortener.Models;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.MapPost("api/shorten", (ShortenUrlRequest request) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
    {
        return Results.BadRequest("The specified URL is invalid.");
    }



    return Results.Ok();
});

app.UseHttpsRedirection();

app.Run();