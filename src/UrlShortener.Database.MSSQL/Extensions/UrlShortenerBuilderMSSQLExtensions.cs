using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core;
using UrlShortener.Core.DatabaseInitializer;
using UrlShortener.Core.Helpers.SQLHelpers;
using UrlShortener.Core.Repository;
using UrlShortener.Core.Services.DatabaseBackground;
using UrlShortener.Database.MSSQL.Helpers.SQLHelpers;
using UrlShortener.Database.MSSQL.Repository;

namespace UrlShortener.Database.MSSQL.Extensions;
public static class UrlShortenerBuilderMSSQLExtensions
{
    /// <summary>
    /// Configures the necessary services for using Microsoft SQL Server (MSSQL) as the storage mechanism 
    /// for the URL shortener service. This method registers the required SQL-related dependencies 
    /// including the <see cref="ISQLHelper"/> and <see cref="IShortenedUrlRepository"/> implementations.
    /// 
    /// It also triggers any additional setup needed after the storage type is configured by calling 
    /// the <see cref="UrlShortenerBuilder.AfterStorageTypeSet"/> method. This is typically used to ensure proper 
    /// initialization and background processing are in place for the SQL-based storage.
    ///
    /// The following services are registered:
    /// <list type="bullet">
    ///   <item><description><see cref="ISQLHelper"/> - Provides SQL-related helper functions for managing 
    ///   interactions with the database, such as executing queries and transactions.</description></item>
    ///   <item><description><see cref="IShortenedUrlRepository"/> - The repository interface for 
    ///   managing shortened URL records in the database.</description></item>
    /// </list>
    /// 
    /// Additionally, after configuring these services, the <see cref="UrlShortenerBuilder.AfterStorageTypeSet"/> method 
    /// is called to ensure any further setup (such as database initialization and background services) is completed.
    /// </summary>
    /// <param name="builder">The <see cref="UrlShortenerBuilder"/> instance used for configuring the URL shortener service.</param>

    public static void AddMSSQL(this UrlShortenerBuilder builder)
    {
        builder.services.AddSingleton<ISQLHelper, MSSQLHelper>();
        builder.services.AddScoped<IShortenedUrlRepository, MSSQLShortenedUrlRepository>();

        builder.AfterStorageTypeSet();
    }
}
