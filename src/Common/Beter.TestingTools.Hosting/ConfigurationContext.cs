using Microsoft.Extensions.Configuration;

namespace Beter.TestingTools.Hosting;

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
