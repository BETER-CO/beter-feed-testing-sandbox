using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.SignalR.Hubs;

/// <summary>
/// A feed client against the four hubs of the host: the connection with <c>ApiKey</c> and
/// <c>snapshotBatchSize</c>, the empty snapshot and the heartbeat, over both protocols.
/// </summary>
public class HubConnectionTests : IClassFixture<SandboxHost>
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(15);

    private readonly SandboxHost _host;

    public HubConnectionTests(SandboxHost host)
    {
        _host = host;
    }

    public static TheoryData<string, string> HubsAndProtocols
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var hub in new[] { "/time_table", "/trading", "/scoreboard", "/incident", "/v1/trading" })
            {
                data.Add(hub, "json");
                data.Add(hub, "messagepack");
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(HubsAndProtocols))]
    public async Task Hub_SendsAnEmptySnapshotAndAHeartbeat(string hub, string protocol)
    {
        var snapshot = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var heartbeat = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var connection = Connect($"{hub}?ApiKey={Guid.NewGuid()}&snapshotBatchSize=10", protocol);
        connection.On<object[]>(HubMethods.OnUpdate, items => snapshot.TrySetResult(items.Length));
        connection.On<long>(HubMethods.OnHeartbeat, sentAt => heartbeat.TrySetResult(sentAt));

        await connection.StartAsync();

        Assert.Equal(0, await snapshot.Task.WaitAsync(Patience));
        Assert.True(await heartbeat.Task.WaitAsync(Patience) > 0);
    }

    [Theory]
    [InlineData("/trading")]
    [InlineData("/trading?ApiKey=not-a-guid")]
    [InlineData("/trading?ApiKey=00000000-0000-0000-0000-000000000000")]
    public async Task Hub_ClosesAConnectionWithoutAnApiKey(string url)
    {
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var snapshot = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var connection = Connect(url, "json");
        connection.On<object[]>(HubMethods.OnUpdate, _ => snapshot.TrySetResult());
        connection.Closed += _ =>
        {
            closed.TrySetResult();
            return Task.CompletedTask;
        };

        await connection.StartAsync();

        await closed.Task.WaitAsync(Patience);
        Assert.False(snapshot.Task.IsCompleted);
    }

    private HubConnection Connect(string url, string protocol)
    {
        var builder = new HubConnectionBuilder().WithUrl(new Uri(_host.Server.BaseAddress, url), options =>
        {
            options.HttpMessageHandlerFactory = _ => _host.Server.CreateHandler();
            options.Transports = HttpTransportType.LongPolling;
        });

        if (protocol == "messagepack")
        {
            builder.AddMessagePackProtocol(options =>
                options.SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));
        }

        return builder.Build();
    }
}
