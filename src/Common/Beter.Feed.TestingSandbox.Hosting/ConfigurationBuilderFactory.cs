using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Beter.Feed.TestingSandbox.Hosting;

public static class ConfigurationBuilderFactory
{
    public static string? GetEnvironment(string[] args)
    {
        var initialConfigBuilder = new ConfigurationBuilder().AddEnvironmentVariables("ASPNETCORE_").AddCommandLine(args);
        var initialConfig = (IConfiguration)initialConfigBuilder.Build();

        return string.IsNullOrEmpty(initialConfig[HostDefaults.EnvironmentKey])
            ? Environment.GetEnvironmentVariable("Hosting:Environment") ??
              Environment.GetEnvironmentVariable("ASPNET_ENV")
            : initialConfig[HostDefaults.EnvironmentKey];
    }

    public static ConfigurationContext Create(
        string[] args,
        params string[] configFileParts)
    {
        return Create(args, null, configFileParts);
    }

    public static ConfigurationContext Create(
        string[] args,
        Action<IConfigurationBuilder, string>? configureDelegate,
        params string[] configFileParts)
    {
        var environment = GetEnvironment(args) ?? "development";

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory);

        foreach (var current in configFileParts ?? Array.Empty<string>())
        {
            configurationBuilder.AddJsonFile(Path.Combine("configs", $"{current}.json"), true);
            configurationBuilder.AddJsonFile(Path.Combine("configs", $"{current}.{environment}.json"), true);
            configurationBuilder.AddJsonFile(Path.Combine("configs", $"{current}.local.json"), true);
        }

        var dotnet_environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (!string.Equals(dotnet_environment, "IntegrationTests", StringComparison.OrdinalIgnoreCase))
        {
            configurationBuilder.AddEnvironmentVariables();
        }

        if (args != null)
        {
            configurationBuilder.AddCommandLine(args);
        }

        configureDelegate?.Invoke(configurationBuilder, environment);

        return new ConfigurationContext(environment, configurationBuilder);
    }
}
