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
/// case set of the image, the served description, and the sample consumer on the four hubs.
/// </summary>
[Trait("Category", "smoke")]
public partial class SandboxSmokeTests : IClassFixture<SandboxContainer>
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    // Case 1001 spans 1681 s of feed time: about 84 s at acceleration 20.
    private static readonly TimeSpan RunPatience = TimeSpan.FromMinutes(4);
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
    public async Task Cases_TheImageCarriesItsCaseSet()
    {
        using var smoke = await GetJson("/api/cases?tag=smoke&status=all");
        using var retired = await GetJson("/api/cases?status=retired");
        using var first = await GetJson($"/api/cases/{smoke.RootElement[0].GetProperty("caseId").GetInt32()}");
        var fields = new[] { "caseId", "version", "status", "class", "sport", "tags", "description", "terminal", "covers", "duration" };

        Assert.NotEqual(0, smoke.RootElement.GetArrayLength());
        Assert.All(smoke.RootElement.EnumerateArray(), row =>
            Assert.All(fields, field => Assert.True(row.TryGetProperty(field, out _), $"A smoke row has no {field}.")));
        Assert.NotEqual(0, retired.RootElement.GetArrayLength());
        Assert.All(retired.RootElement.EnumerateArray(), row => Assert.True(row.TryGetProperty("replacedBy", out _)));
        Assert.Equal("active", first.RootElement.GetProperty("status").GetString());
        Assert.NotEqual(0, first.RootElement.GetProperty("script").GetArrayLength());
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
    [InlineData("GET", "/api/runs/run-1/messages")]
    [InlineData("GET", "/api/runs/run-1/events")]
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

    [Fact]
    public async Task SampleConsumer_ReceivesEveryMessageOfCase1001InOrder_AndAConsumerOfAnotherKeyNone()
    {
        var expected = await ItemCountsOfCaseAsync(1001);
        var apiKey = Guid.NewGuid();
        var sink = new RecordingSink();
        var otherSink = new RecordingSink();
        await using var services = ConsumerServices(sink, apiKey);
        await using var otherServices = ConsumerServices(otherSink, Guid.NewGuid());
        var clients = ClientsOf(services).Concat(ClientsOf(otherServices)).ToArray();
        using var timeout = new CancellationTokenSource(Patience);

        try
        {
            await Task.WhenAll(clients.Select(client => client.ConnectAsync(timeout.Token)));
            foreach (var channel in Channels)
            {
                await sink.First(channel, HubMethods.OnUpdate, Patience);
                await otherSink.First(channel, HubMethods.OnUpdate, Patience);
            }

            using var created = await PostJson("/api/runs", $$"""{"caseId":1001,"apiKey":"{{apiKey}}","acceleration":20}""");
            await WaitForCompletedAsync(created.RootElement.GetProperty("runId").GetString()!, RunPatience);
            foreach (var channel in Channels)
            {
                await WaitUntilAsync(() => sink.UpdateCounts(channel).Count >= expected[channel].Length, Patience);
            }

            foreach (var channel in Channels)
            {
                Assert.Equal(expected[channel], sink.UpdateCounts(channel));
                Assert.Empty(otherSink.UpdateCounts(channel));
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

    private static IFeedClient[] ClientsOf(ServiceProvider services) =>
    [
        services.GetRequiredService<TimeTableFeedServiceClient>(),
        services.GetRequiredService<TradingFeedServiceClient>(),
        services.GetRequiredService<ScoreboardFeedServiceClient>(),
        services.GetRequiredService<IncidentFeedServiceClient>(),
    ];

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan patience)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            Assert.True(watch.Elapsed < patience, $"The consumer did not receive every message within {patience}.");
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }

    private ServiceProvider ConsumerServices(RecordingSink sink, Guid apiKey) => new ServiceCollection()
        .AddSingleton<IFeedMessageSink>(sink)
        .AddFeedServiceClients(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeedService:Host"] = _sandbox.BaseAddress.ToString().TrimEnd('/'),
                ["FeedService:ApiKey"] = apiKey.ToString(),
                ["FeedService:ReconnectionWaitSeconds"] = "0.5",
                ["FeedService:Protocol"] = FeedServiceOptions.JsonProtocol,
            })
            .Build())
        .BuildServiceProvider();

    /// <summary>The item count of every message entry of a case, per channel, in <c>at</c> order.</summary>
    private async Task<Dictionary<string, int[]>> ItemCountsOfCaseAsync(int caseId)
    {
        await using var body = await _sandbox.Client.GetStreamAsync(new Uri($"/api/cases/{caseId}", UriKind.Relative));
        using var file = await JsonDocument.ParseAsync(body);
        var entries = file.RootElement.GetProperty("script").EnumerateArray()
            .Where(entry => entry.TryGetProperty("messageType", out var type) && Channels.Contains(type.GetString()))
            .OrderBy(entry => entry.GetProperty("at").GetInt64())
            .ToArray();
        return Channels.ToDictionary(
            channel => channel,
            channel => entries
                .Where(entry => entry.GetProperty("messageType").GetString() == channel)
                .Select(entry => entry.GetProperty("value").GetArrayLength())
                .ToArray());
    }

    private async Task WaitForCompletedAsync(string runId, TimeSpan patience)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (true)
        {
            using var run = await GetJson($"/api/runs/{runId}");
            var status = run.RootElement.GetProperty("status").GetString();
            if (status is "completed" or "stopped" or "failed" || watch.Elapsed > patience)
            {
                Assert.True(status == "completed", $"Run {runId} is {status}: {run.RootElement}");
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private async Task<JsonDocument> PostJson(string path, string body)
    {
        using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        using var response = await _sandbox.Client.PostAsync(new Uri(path, UriKind.Relative), content);
        var text = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, $"POST {path} answered {(int)response.StatusCode}: {text}");
        return JsonDocument.Parse(text);
    }

    private async Task<JsonDocument> GetJson(string path)
    {
        using var response = await _sandbox.Client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
