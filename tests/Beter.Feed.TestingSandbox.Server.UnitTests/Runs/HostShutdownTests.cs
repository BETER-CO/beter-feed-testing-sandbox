using Beter.Feed.TestingSandbox.Server.Runs;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Runs;

/// <summary>A host of its own, stopped while it holds a waiting and a playing run.</summary>
public class HostShutdownTests
{
    [Fact]
    public async Task StoppingTheHost_EndsAWaitingAndAPlayingRunAsStopped()
    {
        var host = new SandboxHost();
        await host.InitializeAsync();
        var caseId = await RunsClient.LoadCaseAsync(host, "forty-seconds");
        var waiting = await RunsClient.StartAsync(host, new { caseId, apiKey = Guid.NewGuid().ToString() });
        var playing = await RunsClient.StartAsync(host, new { caseId, apiKey = Guid.NewGuid().ToString(), start = "now" });
        var runs = host.Services.GetRequiredService<RunRegistry>().List(null);

        await host.DisposeAsync();

        Assert.Equal("waiting", waiting.GetProperty("status").GetString());
        Assert.Equal("playing", playing.GetProperty("status").GetString());
        Assert.Equal(2, runs.Count);
        Assert.All(runs, run => Assert.Equal(RunStatus.Stopped, run.Status));
        Assert.All(runs, run => Assert.True(run.Playback.IsCompleted));
    }

    [Fact]
    public async Task RunCreated_AfterTheHostBeganToStop_IsCreatedStopped()
    {
        var host = new SandboxHost();
        await host.InitializeAsync();
        try
        {
            var caseId = await RunsClient.LoadCaseAsync(host, "forty-seconds");
            await host.Services.GetRequiredService<RunRegistry>().StopAsync(CancellationToken.None);

            var run = await RunsClient.StartAsync(host, new { caseId, apiKey = Guid.NewGuid().ToString(), start = "now" });

            Assert.Equal("stopped", run.GetProperty("status").GetString());
            Assert.Equal(System.Text.Json.JsonValueKind.String, run.GetProperty("finishedAt").ValueKind);
        }
        finally
        {
            await host.DisposeAsync();
        }
    }
}
