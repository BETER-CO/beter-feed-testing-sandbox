using Beter.Feed.TestingSandbox.Hosting;
using Beter.Feed.TestingSandbox.Server.Cases;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;

/// <summary>
/// The host as <c>Program</c> starts it, on an in-memory server: the same configuration
/// file, the same <see cref="Startup"/>, the heartbeat shortened to one second, and the
/// small case set of <c>Fixtures/cases</c> in place of the case set of the image.
/// </summary>
public class SandboxHost : IAsyncLifetime
{
    private IHost _host;

    /// <summary>Gets the case set the tests of the operations read.</summary>
    public static string FixtureCases => Path.Combine(AppContext.BaseDirectory, "Fixtures", "cases");

    public HttpClient Client { get; private set; }

    public TestServer Server => _host.GetTestServer();

    public IServiceProvider Services => _host.Services;

    public CaseCatalogue Catalogue => Services.GetRequiredService<CaseCatalogue>();

    /// <summary>Gets the directory of the case set; null keeps the one the host is configured with.</summary>
    protected virtual string CaseDirectory => FixtureCases;

    /// <summary>Gets a value indicating whether the host loads its case set at start, as <c>Program</c> does.</summary>
    protected virtual bool LoadsCaseSet => true;

    /// <summary>Gets a value indicating whether the fixture is ready only once the case set is loaded.</summary>
    protected virtual bool WaitsForCaseSet => true;

    public async Task InitializeAsync()
    {
        var settings = new Dictionary<string, string> { ["Heartbeat:IntervalInSeconds"] = "1" };
        if (CaseDirectory is not null)
        {
            settings["Cases:Directory"] = CaseDirectory;
        }

        var configuration = ConfigurationBuilderFactory.Create(Array.Empty<string>(), "appsettings")
            .ConfigurationBuilder
            .AddInMemoryCollection(settings)
            .Build();

        _host = WebAppBuilder.CreateWebHostBuilder<Startup>(configuration)
            .ConfigureWebHost(web => web.UseTestServer())
            .ConfigureServices(services =>
            {
                if (!LoadsCaseSet)
                {
                    var loader = services.Single(service => service.ImplementationType == typeof(CaseSetLoader));
                    services.Remove(loader);
                }
            })
            .Build();
        await _host.StartAsync();
        Client = _host.GetTestClient();
        if (LoadsCaseSet && WaitsForCaseSet)
        {
            await Catalogue.Loaded.WaitAsync(TimeSpan.FromMinutes(2));
        }
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }
}
