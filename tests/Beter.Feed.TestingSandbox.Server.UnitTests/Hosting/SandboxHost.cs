using Beter.Feed.TestingSandbox.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;

/// <summary>
/// The host as <c>Program</c> starts it, on an in-memory server: the same configuration
/// file, the same <see cref="Startup"/>, the heartbeat shortened to one second.
/// </summary>
public sealed class SandboxHost : IAsyncLifetime
{
    private IHost _host;

    public HttpClient Client { get; private set; }

    public TestServer Server => _host.GetTestServer();

    public IServiceProvider Services => _host.Services;

    public async Task InitializeAsync()
    {
        var configuration = ConfigurationBuilderFactory.Create(Array.Empty<string>(), "appsettings")
            .ConfigurationBuilder
            .AddInMemoryCollection(new Dictionary<string, string> { ["Heartbeat:IntervalInSeconds"] = "1" })
            .Build();

        _host = WebAppBuilder.CreateWebHostBuilder<Startup>(configuration)
            .ConfigureWebHost(web => web.UseTestServer())
            .Build();
        await _host.StartAsync();
        Client = _host.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }
}
