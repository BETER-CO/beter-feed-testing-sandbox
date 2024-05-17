using Microsoft.Extensions.Configuration;

namespace Beter.Feed.TestingSandbox.Hosting;

public class ConfigurationContext
{
    public ConfigurationContext(string environment, IConfigurationBuilder configurationBuilder)
    {
        Environment = environment;
        ConfigurationBuilder = configurationBuilder;
    }

    public string Environment { get; }

    public IConfigurationBuilder ConfigurationBuilder { get; }
}
