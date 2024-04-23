using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Beter.TestingTools.Logging.WebApi;

namespace Beter.TestingTools.Hosting;

public static class WebAppBuilder
{
    public static IHostBuilder CreateWebHostBuilder<TStartup>(
        IConfigurationRoot configuration)
        where TStartup : class
    {

        var builder = Host.CreateDefaultBuilder()
            .AddLogging()
            .ConfigureWebHostDefaults(configure =>
            {
                configure
                    .UseConfiguration(configuration)
                    .UseKestrel(
                        (context, options) =>
                        {
                            options.Configure(context.Configuration.GetSection("Kestrel"));
                            options.AddServerHeader = false;
                            options.Limits.MaxRequestLineSize = 1048576;
                        })
                    .ConfigureAppConfiguration((_, config) => { config.AddConfiguration(configuration); })
                    .UseDefaultServiceProvider((context, options) =>
                        options.ValidateScopes = context.HostingEnvironment.IsDevelopment())
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureServices((_, services) =>
                    {
                        services.AddWebApi();
                    })
                    .UseStartup<TStartup>()
                ;
            });

        return builder;
    }
}
