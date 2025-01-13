using UrlShortener;
using UrlShortener.Database.MySQL.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();
builder.Services.AddUrlShortener().AddConfiguration(builder.Configuration).AddMySQL();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
}

app.UseUrlShortener();

app.UseHttpsRedirection();

app.Run();