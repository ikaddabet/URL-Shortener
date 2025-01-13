using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UrlShortener.Core.DatabaseInitializer;
using UrlShortener.Core.Services.DatabaseBackground;

namespace UrlShortener.Core;
public class UrlShortenerBuilder(IServiceCollection services)
{
    public IServiceCollection services = services;

    /// <summary>
    /// Configures additional services required for the URL shortener after the storage type is set.
    /// This method is typically called after the storage mechanism (such as MSSQL) has been configured
    /// in the dependency injection container, allowing for further configuration related to database 
    /// initialization and background processing tasks.
    ///
    /// The following services are registered:
    /// <list type="bullet">
    ///   <item><description><see cref="IUrlShortenerDatabaseInitializer"/> - Responsible for initializing 
    ///   the database with necessary schema or data.</description></item>
    ///   <item><description><see cref="DatabaseBackgroundService"/> - A hosted service that manages 
    ///   background tasks related to the database (e.g., maintenance, cleanup, periodic operations).</description></item>
    /// </list>
    /// </summary>
    public void AfterStorageTypeSet() {
        services.AddSingleton<IUrlShortenerDatabaseInitializer, UrlShortenerDatabaseInitializer>();
        services.AddHostedService<DatabaseBackgroundService>();
    }
}

public class UrlShortenerBuilderOptions(IServiceCollection services) : UrlShortenerBuilder(services)
{
    /// <summary>
    /// Configures the URL shortener options using the provided <paramref name="configuration"/>.
    /// This method checks if the <see cref="IOptions{UrlShortenerOptions}"/> service is already 
    /// registered in the service collection. If it exists, the existing configuration is removed 
    /// and replaced with the new configuration values from the provided <paramref name="configuration"/> 
    /// object, specifically from the "UrlShortenerOptions" section of the configuration.
    ///
    /// This method ensures that the latest configuration settings are applied to the URL shortener
    /// options, which may include custom configurations for the URL shortening service.
    ///
    /// <para>
    /// The following steps are performed in this method:
    /// <list type="number">
    ///   <item><description>Checks if the <see cref="IOptions{UrlShortenerOptions}"/> service is 
    ///   already registered in the services collection.</description></item>
    ///   <item><description>If found, removes the existing <see cref="IOptions{UrlShortenerOptions}"/> 
    ///   configuration from the services collection.</description></item>
    ///   <item><description>Registers the new configuration for <see cref="UrlShortenerOptions"/> 
    ///   using the provided <paramref name="configuration"/> object.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="configuration">The configuration object containing the URL shortener options to be applied.</param>
    /// <returns>Returns the current instance of <see cref="UrlShortenerBuilder"/> to allow for method chaining.</returns>
    public UrlShortenerBuilder AddConfiguration(IConfiguration configuration)
    {
        ServiceDescriptor? isConfigured = services.FirstOrDefault(service => service.ServiceType == typeof(IOptions<UrlShortenerOptions>));
        if (isConfigured is not null)
        {
            bool isRemoved = services.Remove(isConfigured);
            if (isRemoved)
            {
                services.Configure<UrlShortenerOptions>(configuration.GetSection("UrlShortenerOptions"));
            }
        }

        return new UrlShortenerBuilder(services);
    }

}
