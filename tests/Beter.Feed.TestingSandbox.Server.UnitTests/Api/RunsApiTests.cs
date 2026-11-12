using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Server.Api;
using Beter.Feed.TestingSandbox.Server.Description;
using Beter.Feed.TestingSandbox.Server.Runs;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using Beter.Feed.TestingSandbox.Server.UnitTests.Runs;
using LateApexEarlySpeed.Json.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Api;

/// <summary>
/// The five run operations: their errors, the manifest, the list and its filter, stop and
/// delete. Case 7001 of <c>Fixtures/cases</c> plays for 61.5 s at acceleration 1.
/// </summary>
public partial class RunsApiTests : IClassFixture<SandboxHost>
{
    private const string Key = "6f1c2a3b-4d5e-4f60-8a7b-9c0d1e2f3a4b";

    private static readonly JsonValidator RunManifest =
        new(ContractResources.LoadSchemas()["../schemas/run-output/v1/run.schema.json"].ToJsonString());

    private static readonly JsonValidator Error = ContractComponent.Validator("Error");
    private static readonly JsonValidator RetiredError = ContractComponent.Validator("RetiredError");

    private readonly SandboxHost _host;

    public RunsApiTests(SandboxHost host)
    {
        _host = host;
    }

    public static TheoryData<string, string, string> RefusedBodies => new()
    {
        { "not an object", "[]", "invalid_body" },
        { "malformed JSON", "{\"caseId\":7001,", "invalid_body" },
        { "no caseId", $$"""{"apiKey":"{{Key}}"}""", "invalid_case_id" },
        { "caseId a string", $$"""{"caseId":"7001","apiKey":"{{Key}}"}""", "invalid_case_id" },
        { "caseId 0", $$"""{"caseId":0,"apiKey":"{{Key}}"}""", "invalid_case_id" },
        { "no apiKey", """{"caseId":7001}""", "invalid_api_key" },
        { "apiKey a number", """{"caseId":7001,"apiKey":5}""", "invalid_api_key" },
        { "unknown property", $$"""{"caseId":7001,"apiKey":"{{Key}}","speed":2}""", "unknown_property" },
        { "apiKey not a GUID", """{"caseId":7001,"apiKey":"not-a-guid"}""", "invalid_api_key" },
        { "apiKey in braces", """{"caseId":7001,"apiKey":"{6f1c2a3b-4d5e-4f60-8a7b-9c0d1e2f3a4b}"}""", "invalid_api_key" },
        { "apiKey as 32 digits", """{"caseId":7001,"apiKey":"6f1c2a3b4d5e4f608a7b9c0d1e2f3a4b"}""", "invalid_api_key" },
        { "apiKey the empty GUID", """{"caseId":7001,"apiKey":"00000000-0000-0000-0000-000000000000"}""", "invalid_api_key" },
        { "unknown start", $$"""{"caseId":7001,"apiKey":"{{Key}}","start":"later"}""", "invalid_start" },
        { "waitTimeoutSeconds 0", $$"""{"caseId":7001,"apiKey":"{{Key}}","waitTimeoutSeconds":0}""", "invalid_wait_timeout" },
        { "acceleration a string", $$"""{"caseId":7001,"apiKey":"{{Key}}","acceleration":"fast"}""", "invalid_acceleration" },
        { "acceleration below 0.25", $$"""{"caseId":7001,"apiKey":"{{Key}}","acceleration":0.2}""", "invalid_acceleration" },
        { "acceleration above the default maxAcceleration", $$"""{"caseId":7001,"apiKey":"{{Key}}","acceleration":20.5}""", "invalid_acceleration" },
    };

    [Theory]
    [MemberData(nameof(RefusedBodies))]
    public async Task StartRun_RefusesABadBody_With400AndAnErrorBody(string cause, string body, string error)
    {
        var (status, answer) = await RunsClient.SendAsync(_host, HttpMethod.Post, "/api/runs", body);

        Assert.True(status == HttpStatusCode.BadRequest, $"{cause}: {(int)status} {answer}");
        AssertError(answer, error);
    }

    [Fact]
    public async Task StartRun_RefusesAnAccelerationAboveTheMaxAccelerationOfTheCase()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "limited");

        var (status, answer) = await RunsClient.SendAsync(
            _host, HttpMethod.Post, "/api/runs", JsonSerializer.Serialize(new { caseId, apiKey = Key, acceleration = 2.5 }));

        Assert.Equal(HttpStatusCode.BadRequest, status);
        AssertError(answer, "invalid_acceleration");
    }

    [Fact]
    public async Task StartRun_OfAnUnknownCase_Returns404()
    {
        var (status, answer) = await RunsClient.SendAsync(_host, HttpMethod.Post, "/api/runs", $$"""{"caseId":9999,"apiKey":"{{Key}}"}""");

        Assert.Equal(HttpStatusCode.NotFound, status);
        AssertError(answer, "not_found");
    }

    [Theory]
    [InlineData(7003, 7001)]
    [InlineData(7009, null)]
    public async Task StartRun_OfARetiredCase_Returns410NamingTheReplacement(int caseId, int? replacedBy)
    {
        var (status, answer) = await RunsClient.SendAsync(_host, HttpMethod.Post, "/api/runs", $$"""{"caseId":{{caseId}},"apiKey":"{{Key}}"}""");

        Assert.Equal(HttpStatusCode.Gone, status);
        AssertError(answer, "retired");
        Assert.Null(RetiredError.Failure(answer));
        var replacement = answer.GetProperty("replacedBy");
        Assert.Equal(replacedBy, replacement.ValueKind == JsonValueKind.Null ? null : replacement.GetInt32());
    }

    [Theory]
    [InlineData("GET", "/api/runs/r-20260101-0000-0000")]
    [InlineData("POST", "/api/runs/r-20260101-0000-0000/stop")]
    [InlineData("DELETE", "/api/runs/r-20260101-0000-0000")]
    public async Task RunOperation_OfAnUnknownRun_Returns404(string method, string path)
    {
        var (status, answer) = await RunsClient.SendAsync(_host, new HttpMethod(method), path, null);

        Assert.Equal(HttpStatusCode.NotFound, status);
        AssertError(answer, "not_found");
    }

    [Fact]
    public async Task ListRuns_WithAnUnknownStatus_Returns400()
    {
        var (status, answer) = await RunsClient.SendAsync(_host, HttpMethod.Get, "/api/runs?status=passed", null);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        AssertError(answer, "invalid_status");
    }

    [Fact]
    public async Task StartRun_Returns201WithTheManifest()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "four-channels");
        var apiKey = Guid.NewGuid();

        var manifest = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString("D").ToUpperInvariant(), acceleration = 2 });

        Assert.Null(RunManifest.Failure(manifest));
        Assert.Matches(RunId(), manifest.GetProperty("runId").GetString());
        Assert.Equal(1, manifest.GetProperty("outputVersion").GetInt32());
        Assert.Equal(apiKey.ToString("D"), manifest.GetProperty("apiKey").GetString());
        Assert.Equal(SandboxVersion.OfThisBuild().Value, manifest.GetProperty("sandboxVersion").GetString());
        Assert.Equal("2026-10-31", manifest.GetProperty("contract").GetProperty("fetchedAt").GetString());
        Assert.Equal(
            $$"""{"caseId":{{caseId}},"version":"1.0.0","class":"baseline","sport":6,"tags":["fixture","baseline"],"terminal":"Finished"}""",
            manifest.GetProperty("case").GetRawText());
        Assert.Equal(2, manifest.GetProperty("acceleration").GetDouble());
        Assert.Equal("onConnect", manifest.GetProperty("start").GetString());
        Assert.Equal("waiting", manifest.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, manifest.GetProperty("error").ValueKind);
        Assert.Equal(JsonValueKind.Null, manifest.GetProperty("startedAt").ValueKind);
        Assert.Equal(JsonValueKind.Null, manifest.GetProperty("finishedAt").ValueKind);
        Assert.Equal("""["EX-100-1"]""", manifest.GetProperty("expected").GetRawText());
        Assert.Equal(
            """{"messages":{},"offsets":{},"matches":[],"directives":[],"connections":0,"snapshots":{"connection":0,"recovery":0,"booking":0}}""",
            manifest.GetProperty("summary").GetRawText());
    }

    [Fact]
    public async Task Manifest_OfACaseWithoutContractSportOrExpected_ValidatesAndTakesTheStartOfTheCase()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "limited");

        var manifest = await RunsClient.StartAsync(_host, new { caseId, apiKey = Guid.NewGuid().ToString(), acceleration = 2 });
        var completed = await RunsClient.WaitForStatusAsync(_host, manifest.GetProperty("runId").GetString(), "completed", RunsClient.Patience);

        Assert.Null(RunManifest.Failure(manifest));
        Assert.Null(RunManifest.Failure(completed));
        Assert.Equal("now", manifest.GetProperty("start").GetString());
        Assert.Equal(JsonValueKind.Null, manifest.GetProperty("contract").ValueKind);
        Assert.False(manifest.GetProperty("case").TryGetProperty("sport", out _));
        Assert.Equal("[]", manifest.GetProperty("expected").GetRawText());
        Assert.Equal(manifest.GetProperty("createdAt").GetString(), completed.GetProperty("startedAt").GetString());
        Assert.True(RunsClient.Instant(completed, "finishedAt") >= RunsClient.Instant(completed, "startedAt"));
    }

    [Fact]
    public async Task ListRuns_ListsEveryRunOldestFirst_AndFiltersByStatus()
    {
        var first = await RunsClient.StartAsync(_host, new { caseId = 7001, apiKey = Guid.NewGuid().ToString(), waitTimeoutSeconds = 600 });
        var second = await RunsClient.StartAsync(_host, new { caseId = 7001, apiKey = Guid.NewGuid().ToString(), start = "now" });
        var ids = new[] { first, second }.Select(run => run.GetProperty("runId").GetString()).ToArray();

        var (_, all) = await RunsClient.SendAsync(_host, HttpMethod.Get, "/api/runs", null);
        var (_, waiting) = await RunsClient.SendAsync(_host, HttpMethod.Get, "/api/runs?status=waiting", null);
        var (_, playing) = await RunsClient.SendAsync(_host, HttpMethod.Get, "/api/runs?status=playing", null);
        var (_, completed) = await RunsClient.SendAsync(_host, HttpMethod.Get, "/api/runs?status=completed", null);

        Assert.Equal(ids, RunIds(all).Where(ids.Contains));
        Assert.All(all.EnumerateArray(), manifest => Assert.Null(RunManifest.Failure(manifest)));
        Assert.Contains(ids[0], RunIds(waiting));
        Assert.DoesNotContain(ids[1], RunIds(waiting));
        Assert.Contains(ids[1], RunIds(playing));
        Assert.All(waiting.EnumerateArray(), manifest => Assert.Equal("waiting", manifest.GetProperty("status").GetString()));
        Assert.All(playing.EnumerateArray(), manifest => Assert.Equal("playing", manifest.GetProperty("status").GetString()));
        Assert.DoesNotContain(ids[0], RunIds(completed));

        foreach (var id in ids)
        {
            await RunsClient.SendAsync(_host, HttpMethod.Post, $"/api/runs/{id}/stop", null);
        }
    }

    [Fact]
    public async Task StopRun_ReturnsTheManifestStopped_AndNoFurtherMessageArrives()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "late-second");
        var apiKey = Guid.NewGuid();
        await using var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);
        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString() });
        var runId = run.GetProperty("runId").GetString();
        await consumer.WaitForUpdatesAsync(ChannelNames.Trading, 1, RunsClient.Patience);

        var (status, stopped) = await RunsClient.SendAsync(_host, HttpMethod.Post, $"/api/runs/{runId}/stop", null);
        await _host.Services.GetRequiredService<RunRegistry>().Find(runId).Playback.WaitAsync(RunsClient.Patience);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Null(RunManifest.Failure(stopped));
        Assert.Equal("stopped", stopped.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.String, stopped.GetProperty("finishedAt").ValueKind);
        Assert.Single(consumer.Updates(ChannelNames.Trading));
        Assert.Equal("stopped", (await RunsClient.GetAsync(_host, runId)).GetProperty("status").GetString());
    }

    [Fact]
    public async Task StartRun_TakesAnUppercaseKey_AndPlaysToAConnectionPresentingItInLowercase()
    {
        var caseId = await RunsClient.LoadCaseAsync(_host, "second-match");
        var apiKey = Guid.NewGuid();
        await using var consumer = await FeedConsumer.ConnectAsync(_host, apiKey);

        var run = await RunsClient.StartAsync(_host, new { caseId, apiKey = apiKey.ToString("D").ToUpperInvariant(), acceleration = 20 });
        await RunsClient.WaitForStatusAsync(_host, run.GetProperty("runId").GetString(), "completed", RunsClient.Patience);

        Assert.Equal(apiKey.ToString("D"), run.GetProperty("apiKey").GetString());
        await consumer.WaitForUpdatesAsync(ChannelNames.Incident, 1, RunsClient.Patience);
    }

    [Fact]
    public async Task StopAndDelete_RacingOnManyRuns_NeverAnswer500()
    {
        var runs = await Task.WhenAll(Enumerable.Range(0, 40).Select(_ =>
            RunsClient.StartAsync(_host, new { caseId = 7001, apiKey = Guid.NewGuid().ToString(), start = "now" })));

        var answers = await Task.WhenAll(runs.Select(run => run.GetProperty("runId").GetString()).SelectMany(runId => new[]
        {
            RunsClient.SendAsync(_host, HttpMethod.Post, $"/api/runs/{runId}/stop", null),
            RunsClient.SendAsync(_host, HttpMethod.Delete, $"/api/runs/{runId}", null),
            RunsClient.SendAsync(_host, HttpMethod.Post, $"/api/runs/{runId}/stop", null),
            RunsClient.SendAsync(_host, HttpMethod.Delete, $"/api/runs/{runId}", null),
        }));

        Assert.All(answers, answer => Assert.True(
            answer.status is HttpStatusCode.OK or HttpStatusCode.NoContent or HttpStatusCode.Conflict or HttpStatusCode.NotFound,
            $"Answered {(int)answer.status}: {answer.body}"));
    }

    [Fact]
    public async Task StopRun_OfATerminalRun_Returns409()
    {
        var run = await RunsClient.StartAsync(_host, new { caseId = 7001, apiKey = Guid.NewGuid().ToString() });
        var runId = run.GetProperty("runId").GetString();
        await RunsClient.SendAsync(_host, HttpMethod.Post, $"/api/runs/{runId}/stop", null);

        var (status, answer) = await RunsClient.SendAsync(_host, HttpMethod.Post, $"/api/runs/{runId}/stop", null);

        Assert.Equal(HttpStatusCode.Conflict, status);
        AssertError(answer, "run_finished");
    }

    [Fact]
    public async Task DeleteRun_OfALiveRun_Returns409_AndOfATerminalRun_Returns204ThenTheIdIsUnknown()
    {
        var run = await RunsClient.StartAsync(_host, new { caseId = 7001, apiKey = Guid.NewGuid().ToString() });
        var runId = run.GetProperty("runId").GetString();

        var (live, conflict) = await RunsClient.SendAsync(_host, HttpMethod.Delete, $"/api/runs/{runId}", null);
        await RunsClient.SendAsync(_host, HttpMethod.Post, $"/api/runs/{runId}/stop", null);
        var (deleted, _) = await RunsClient.SendAsync(_host, HttpMethod.Delete, $"/api/runs/{runId}", null);
        var (afterwards, unknown) = await RunsClient.SendAsync(_host, HttpMethod.Get, $"/api/runs/{runId}", null);

        Assert.Equal(HttpStatusCode.Conflict, live);
        AssertError(conflict, "run_live");
        Assert.Equal(HttpStatusCode.NoContent, deleted);
        Assert.Equal(HttpStatusCode.NotFound, afterwards);
        AssertError(unknown, "not_found");
    }

    [GeneratedRegex("^r-[0-9]{8}-[0-9]{4}-[0-9a-f]{4}$")]
    private static partial Regex RunId();

    private static string[] RunIds(JsonElement manifests) =>
        manifests.EnumerateArray().Select(manifest => manifest.GetProperty("runId").GetString()).ToArray();

    private static void AssertError(JsonElement answer, string error)
    {
        Assert.Null(Error.Failure(answer));
        Assert.Equal(error, answer.GetProperty("error").GetString());
        Assert.False(string.IsNullOrWhiteSpace(answer.GetProperty("detail").GetString()));
    }
}
