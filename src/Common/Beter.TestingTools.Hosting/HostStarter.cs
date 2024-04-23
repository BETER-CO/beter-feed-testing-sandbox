using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Beter.TestingTools.Logging;

namespace Beter.TestingTools.Hosting;

public static class HostStarter
{
    public static int Start<T>(
            string[] args,
            string application,
            string service,
            params string[] configFileParts)
            where T : class
    {
        return Start<T>(args, application, service, null, configFileParts);
    }

    public static int Start<T>(
        string[] args,
        string application,
        string service,
        Action<IConfigurationBuilder, string>? configureDelegate,
        params string[] configFileParts)
        where T : class
    {
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        if (configFileParts == null || configFileParts.Length == 0)
        {
            configFileParts = new[]
            {
                    "appsettings",
                };
        }

        var configurationContext = ConfigurationBuilderFactory.Create(args, configureDelegate, configFileParts);
        var configurationBuilder = configurationContext.ConfigurationBuilder;
        var environment = configurationContext.Environment;

        var configuration = configurationBuilder.Build();

        Logger.Initialize(configuration, environment, application, service);

        try
        {
            Logger.Instance.Information("Starting service...");

            var webHostBuilder = WebAppBuilder.CreateWebHostBuilder<T>(configuration);
            using var webHost = webHostBuilder.Build();
            webHost.Start();

            var server = webHost.Services.GetService<IServer>();
            if (server != null)
            {
                var addressFeature = server.Features.Get<IServerAddressesFeature>();

                if (addressFeature != null)
                {
                    foreach (var address in addressFeature.Addresses)
                    {
                        Logger.Instance.Debug($"Kestrel is listening on address: {address}");
                    }
                }
            }

            webHost.WaitForShutdown();

            Logger.Instance.Information("Service stopped");
            return 0;
        }
        catch (Exception ex)
        {
            Logger.Instance.Fatal(ex, "Exception occurred while starting service");
            Thread.Sleep(3000);
            return -1;
        }
        finally
        {
            Logger.Close();
        }
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.Instance.Error(
            e.ExceptionObject as Exception,
            "Current domain: unhandled exception occurred. IsTerminating={IsTerminating}",
            e.IsTerminating);
        if (e.IsTerminating)
        {
            Logger.Close();
        }
    }

    private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logger.Instance.Error(e.Exception, "Unobserved exception occurred");
        e.SetObserved();
    }
}
