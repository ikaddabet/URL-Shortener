using UrlShortener;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();
builder.Services.AddUrlShortener(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    options.DatabaseName = builder.Configuration.GetConnectionString("DatabaseName") ?? throw new InvalidOperationException("Connection string 'DatabaseName' not found.");
}).AddConfiguration(builder.Configuration).AddMSSQL();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
}

app.UseUrlShortener();

app.UseHttpsRedirection();

app.Run();