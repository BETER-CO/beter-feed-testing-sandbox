using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Consumer.Clients;
using Beter.Feed.TestingSandbox.Consumer.Extensions;
using Beter.Feed.TestingSandbox.Consumer.Options;
using Beter.Feed.TestingSandbox.Consumer.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Beter.Feed.TestingSandbox.EndToEndTests;

/// <summary>
/// The end-to-end test through the one container: the version and health operations, the
/// served description, and the sample consumer on the four hubs.
/// </summary>
[Trait("Category", "smoke")]
public partial class SandboxSmokeTests : IClassFixture<SandboxContainer>
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);
    private static readonly string[] Channels =
        [ChannelNames.Timetable, ChannelNames.Trading, ChannelNames.Scoreboard, ChannelNames.Incident];

    private readonly SandboxContainer _sandbox;

    public SandboxSmokeTests(SandboxContainer sandbox)
    {
        _sandbox = sandbox;
    }

    [Fact]
    public async Task Version_IsTheVersionTheImageWasBuiltWith()
    {
        using var body = await GetJson("/api/version");
        var version = body.RootElement.GetProperty("sandboxVersion").GetString()!;

        Assert.Matches(SemVer(), version);
        if (SandboxContainer.ExpectedVersion is { } expected)
        {
            Assert.Equal(expected, version);
        }

        Assert.Equal(1, body.RootElement.GetProperty("outputVersion").GetInt32());
    }

    [Fact]
    public async Task Health_IsOk()
    {
        using var body = await GetJson("/api/health");

        Assert.Equal("ok", body.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Description_IsServedSelfContainedWithTheSandboxVersion()
    {
        using var version = await GetJson("/api/version");
        var text = await _sandbox.Client.GetStringAsync("/openapi.json");
        using var document = JsonDocument.Parse(text);
        using var rendered = await _sandbox.Client.GetAsync("/swagger");

        Assert.Equal(
            version.RootElement.GetProperty("sandboxVersion").GetString(),
            document.RootElement.GetProperty("info").GetProperty("version").GetString());
        Assert.DoesNotMatch(ExternalReference(), text);
        Assert.Equal(HttpStatusCode.OK, rendered.StatusCode);
        Assert.Equal("text/html", rendered.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("GET", "/api/cases")]
    [InlineData("POST", "/api/runs")]
    [InlineData("GET", "/api/runs/run-1/messages")]
    public async Task OperationNotBuiltYet_Returns501(string method, string path)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        using var response = await _sandbox.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Theory]
    [InlineData(FeedServiceOptions.JsonProtocol)]
    [InlineData(FeedServiceOptions.MessagePackProtocol)]
    public async Task SampleConsumer_ReceivesAnEmptySnapshotAndAHeartbeatOnTheFourHubs(string protocol)
    {
        var sink = new RecordingSink();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeedService:Host"] = _sandbox.BaseAddress.ToString().TrimEnd('/'),
                ["FeedService:ApiKey"] = Guid.NewGuid().ToString(),
                ["FeedService:SnapshotBatchSize"] = "10",
                ["FeedService:ReconnectionWaitSeconds"] = "0.5",
                ["FeedService:Protocol"] = protocol,
            })
            .Build();
        await using var services = new ServiceCollection()
            .AddSingleton<IFeedMessageSink>(sink)
            .AddFeedServiceClients(configuration)
            .BuildServiceProvider();
        IFeedClient[] clients =
        [
            services.GetRequiredService<TimeTableFeedServiceClient>(),
            services.GetRequiredService<TradingFeedServiceClient>(),
            services.GetRequiredService<ScoreboardFeedServiceClient>(),
            services.GetRequiredService<IncidentFeedServiceClient>(),
        ];
        using var timeout = new CancellationTokenSource(Patience);

        try
        {
            await Task.WhenAll(clients.Select(client => client.ConnectAsync(timeout.Token)));

            foreach (var channel in Channels)
            {
                var snapshot = await sink.First(channel, HubMethods.OnUpdate, Patience);
                var heartbeat = await sink.First(channel, HubMethods.OnHeartbeat, Patience);

                Assert.Equal(0, RecordingSink.CountOf(snapshot));
                Assert.True((long)heartbeat > 0, $"{channel} sent heartbeat {heartbeat}.");
            }
        }
        finally
        {
            await Task.WhenAll(clients.Select(client => client.DisconnectAsync(CancellationToken.None)));
        }
    }

    [GeneratedRegex(@"^\d+\.\d+\.\d+(-[0-9A-Za-z.]+)?$")]
    private static partial Regex SemVer();

    [GeneratedRegex("\"\\$ref\"\\s*:\\s*\"[^#]")]
    private static partial Regex ExternalReference();

    private async Task<JsonDocument> GetJson(string path)
    {
        using var response = await _sandbox.Client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
