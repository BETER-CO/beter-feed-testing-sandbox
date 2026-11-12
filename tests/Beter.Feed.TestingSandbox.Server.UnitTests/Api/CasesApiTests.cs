using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using LateApexEarlySpeed.Json.Schema;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Api;

/// <summary>
/// The case operations against the case set of <c>Fixtures/cases</c>: two active cases
/// (7001, 7002), a retired case whose file is kept (7003, replaced by 7001) and a retired id
/// that has no file (7009).
/// </summary>
public class CasesApiTests : IClassFixture<SandboxHost>
{
    private static readonly JsonValidator CatalogueRow = ContractComponent.Validator("CatalogueRow");
    private static readonly JsonValidator CaseDetail = ContractComponent.Validator("CaseDetail");
    private static readonly JsonValidator ValidationError = ContractComponent.Validator("ValidationError");
    private static readonly JsonValidator Error = ContractComponent.Validator("Error");

    private readonly SandboxHost _host;

    public CasesApiTests(SandboxHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task ListCases_WithoutAStatus_ListsTheActiveCasesByTheirId()
    {
        using var rows = await GetJson("/api/cases");

        Assert.Equal(new[] { 7001, 7002 }, Ids(rows).Where(id => id < 100000));
        Assert.All(rows.RootElement.EnumerateArray(), row => Assert.Equal("active", row.GetProperty("status").GetString()));
        Assert.All(rows.RootElement.EnumerateArray(), row => Assert.Null(CatalogueRow.Failure(row)));
    }

    [Fact]
    public async Task ListCases_RowCarriesTheCatalogueFieldsOfTheCaseFile()
    {
        using var rows = await GetJson("/api/cases?tag=efootball");

        var row = Assert.Single(rows.RootElement.EnumerateArray());
        Assert.Equal(7001, row.GetProperty("caseId").GetInt32());
        Assert.Equal("1.2.0", row.GetProperty("version").GetString());
        Assert.Equal("active", row.GetProperty("status").GetString());
        Assert.Equal("baseline", row.GetProperty("class").GetString());
        Assert.Equal(6, row.GetProperty("sport").GetInt32());
        Assert.Equal(new[] { "smoke", "baseline", "efootball" }, Strings(row.GetProperty("tags")));
        Assert.StartsWith("Test fixture", row.GetProperty("description").GetString(), StringComparison.Ordinal);
        Assert.Equal("Finished", row.GetProperty("terminal").GetString());
        Assert.Equal(3, row.GetProperty("covers").GetProperty("count").GetInt32());
        Assert.Equal(
            new[] { "Status", "Market" },
            row.GetProperty("covers").GetProperty("sections").EnumerateArray().Select(section => section.GetProperty("section").GetString()));
        Assert.Equal(61, row.GetProperty("duration").GetInt32());
        Assert.False(row.TryGetProperty("replacedBy", out _));
    }

    [Fact]
    public async Task ListCases_RowOfACaseWithoutASport_HasNoSport_AndTakesTheDurationOfTheFile()
    {
        using var rows = await GetJson("/api/cases?tag=transport-faults");

        var row = Assert.Single(rows.RootElement.EnumerateArray());
        Assert.Equal(7002, row.GetProperty("caseId").GetInt32());
        Assert.False(row.TryGetProperty("sport", out _));
        Assert.Equal(20, row.GetProperty("duration").GetInt32());
        Assert.Equal(0, row.GetProperty("covers").GetProperty("count").GetInt32());
    }

    [Theory]
    [InlineData("/api/cases?tag=smoke", new[] { 7001, 7002 })]
    [InlineData("/api/cases?tag=smoke&tag=baseline", new[] { 7001 })]
    [InlineData("/api/cases?tag=smoke&tag=baseline&status=all", new[] { 7001, 7003 })]
    [InlineData("/api/cases?tag=smoke&status=all", new[] { 7001, 7002, 7003 })]
    [InlineData("/api/cases?tag=smoke&status=retired", new[] { 7003 })]
    [InlineData("/api/cases?status=retired", new[] { 7003, 7009 })]
    [InlineData("/api/cases?status=active&tag=Smoke", new int[0])]
    public async Task ListCases_FiltersByEveryTagAndByStatus(string path, int[] expected)
    {
        using var rows = await GetJson(path);

        Assert.Equal(expected, Ids(rows).Where(id => id < 100000));
        Assert.All(rows.RootElement.EnumerateArray(), row => Assert.Null(CatalogueRow.Failure(row)));
    }

    [Fact]
    public async Task ListCases_RetiredRows_NameTheReplacementOrNull()
    {
        using var rows = await GetJson("/api/cases?status=retired");

        var kept = rows.RootElement[0];
        var withoutFile = rows.RootElement[1];
        Assert.Equal("retired", kept.GetProperty("status").GetString());
        Assert.Equal(7001, kept.GetProperty("replacedBy").GetInt32());
        Assert.Equal("baseline", kept.GetProperty("class").GetString());
        Assert.Equal("""{"caseId":7009,"status":"retired","replacedBy":null}""", withoutFile.GetRawText());
    }

    [Fact]
    public async Task ListCases_WithAnUnknownStatus_Returns400WithAnErrorBody()
    {
        using var response = await _host.Client.GetAsync("/api/cases?status=promoted");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(Error.Failure(body.RootElement));
        Assert.Equal("invalid_status", body.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GetCase_OfAnActiveCase_ReturnsTheCaseFileWithItsStatus()
    {
        using var response = await _host.Client.GetAsync("/api/cases/7001");
        using var served = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var file = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(SandboxHost.FixtureCases, "7001.json")))!.AsObject();
        file["status"] = "active";

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(CaseDetail.Failure(served.RootElement));
        Assert.True(JsonNode.DeepEquals(file, JsonNode.Parse(served.RootElement.GetRawText())));
        Assert.Equal("EX-7001-1", served.RootElement.GetProperty("expected")[0].GetProperty("id").GetString());
        Assert.Equal("Finished", served.RootElement.GetProperty("exposes").GetProperty("terminal").GetString());
    }

    [Fact]
    public async Task GetCase_OfARetiredCaseWhoseFileIsKept_ReturnsTheFileAsRetiredWithItsReplacement()
    {
        using var served = await GetJson("/api/cases/7003");

        Assert.Null(CaseDetail.Failure(served.RootElement));
        Assert.Equal("retired", served.RootElement.GetProperty("status").GetString());
        Assert.Equal(7001, served.RootElement.GetProperty("replacedBy").GetInt32());
        Assert.Equal(1, served.RootElement.GetProperty("script").GetArrayLength());
    }

    [Fact]
    public async Task GetCase_OfARetiredIdWithoutAFile_ReturnsItsIdStatusAndReplacementAlone()
    {
        using var response = await _host.Client.GetAsync("/api/cases/7009");
        var body = await response.Content.ReadAsStringAsync();
        using var served = JsonDocument.Parse(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(CaseDetail.Failure(served.RootElement));
        Assert.Equal("""{"caseId":7009,"status":"retired","replacedBy":null}""", body);
    }

    [Fact]
    public async Task GetCase_OfAnUnknownId_Returns404WithAnErrorBody()
    {
        using var response = await _host.Client.GetAsync("/api/cases/7999");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(Error.Failure(body.RootElement));
        Assert.Equal("not_found", body.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task LoadCase_WithoutAnId_Returns201AndItsRowWithAnIdOfTheRuntimeRange()
    {
        var posted = RuntimeCase();

        using var response = await Post(posted);
        using var row = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var caseId = row.RootElement.GetProperty("caseId").GetInt32();
        using var served = await GetJson($"/api/cases/{caseId}");
        using var listed = await GetJson("/api/cases?tag=runtime-without-id");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Null(CatalogueRow.Failure(row.RootElement));
        Assert.True(caseId >= 100000, $"The assigned id is {caseId}.");
        Assert.Equal("active", row.RootElement.GetProperty("status").GetString());
        Assert.Equal(12, row.RootElement.GetProperty("duration").GetInt32());
        Assert.Null(CaseDetail.Failure(served.RootElement));
        Assert.Equal(caseId, served.RootElement.GetProperty("caseId").GetInt32());
        Assert.Equal(2, served.RootElement.GetProperty("script").GetArrayLength());
        Assert.Equal(new[] { caseId }, Ids(listed));
    }

    [Fact]
    public async Task LoadCase_WithAFreeIdOfTheRuntimeRange_KeepsTheId_AndRefusesItASecondTime()
    {
        var posted = RuntimeCase();
        posted["caseId"] = 100500;
        posted["tags"] = new JsonArray("runtime-with-id");

        using var first = await Post(posted);
        using var row = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        using var second = await Post(posted);
        using var refusal = JsonDocument.Parse(await second.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(100500, row.RootElement.GetProperty("caseId").GetInt32());
        Assert.Equal(HttpStatusCode.UnprocessableEntity, second.StatusCode);
        Assert.Null(ValidationError.Failure(refusal.RootElement));
        var error = Assert.Single(refusal.RootElement.GetProperty("errors").EnumerateArray());
        Assert.Equal("/caseId", error.GetProperty("instanceLocation").GetString());
        Assert.Contains("in use", error.GetProperty("error").GetString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(7001)]
    [InlineData(99999)]
    public async Task LoadCase_WithAnIdBelowTheRuntimeRange_Returns422NamingTheId(int caseId)
    {
        var posted = RuntimeCase();
        posted["caseId"] = caseId;

        using var response = await Post(posted);
        using var refusal = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(ValidationError.Failure(refusal.RootElement));
        Assert.Contains(
            refusal.RootElement.GetProperty("errors").EnumerateArray(),
            error => error.GetProperty("instanceLocation").GetString() == "/caseId");
    }

    [Theory]
    [InlineData("2147483648")]
    [InlineData("100001.0")]
    public async Task LoadCase_WithAnIdThatIsNoWholeNumberOfTheRange_Returns422NamingTheId(string caseId)
    {
        var posted = RuntimeCase().ToJsonString().Replace("{\"version\"", $"{{\"caseId\":{caseId},\"version\"", StringComparison.Ordinal);

        using var content = new StringContent(posted, Encoding.UTF8, "application/json");
        using var response = await _host.Client.PostAsync("/api/cases", content);
        using var refusal = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Contains($"\"caseId\":{caseId}", posted, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(ValidationError.Failure(refusal.RootElement));
        var error = Assert.Single(refusal.RootElement.GetProperty("errors").EnumerateArray());
        Assert.Equal("/caseId", error.GetProperty("instanceLocation").GetString());
    }

    [Fact]
    public async Task LoadCase_WithWholeNumbersWrittenAsDecimals_IsLoaded()
    {
        var posted = RuntimeCase().ToJsonString()
            .Replace("\"at\":12999", "\"at\":12999.0", StringComparison.Ordinal)
            .Replace("{\"version\"", "{\"sport\":6.0,\"version\"", StringComparison.Ordinal)
            .Replace("runtime-without-id", "runtime-decimals", StringComparison.Ordinal);

        using var content = new StringContent(posted, Encoding.UTF8, "application/json");
        using var response = await _host.Client.PostAsync("/api/cases", content);
        using var row = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Contains("12999.0", posted, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Null(CatalogueRow.Failure(row.RootElement));
        Assert.Equal(6, row.RootElement.GetProperty("sport").GetInt32());
        Assert.Equal(12, row.RootElement.GetProperty("duration").GetInt32());
    }

    [Fact]
    public async Task LoadCase_BelowTheRuntimeRange_ReportsTheErrorAtTheCaseIdOfTheCaseSchema()
    {
        var posted = RuntimeCase();
        posted["caseId"] = 5;

        using var response = await Post(posted);
        using var refusal = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(
            new[] { "/caseId @ /$ref/properties/caseId" },
            refusal.RootElement.GetProperty("errors").EnumerateArray().Select(error =>
                $"{error.GetProperty("instanceLocation").GetString()} @ {error.GetProperty("keywordLocation").GetString()}"));
    }

    [Fact]
    public async Task LoadCase_ThatFailsTheCaseSchema_Returns422WithTheSchemaErrors()
    {
        var posted = RuntimeCase();
        posted.Remove("script");
        posted["class"] = "regular";
        posted["messages"] = new JsonArray();

        using var response = await Post(posted);
        using var refusal = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = refusal.RootElement.GetProperty("errors").EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(ValidationError.Failure(refusal.RootElement));
        Assert.Equal("invalid_case", refusal.RootElement.GetProperty("error").GetString());
        Assert.NotEmpty(errors);
        Assert.All(errors, error => Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("error").GetString())));
        Assert.All(errors, error => Assert.StartsWith("/", error.GetProperty("keywordLocation").GetString(), StringComparison.Ordinal));
        Assert.Equal(
            new[] { string.Empty, "/class", "/messages" },
            errors.Select(error => error.GetProperty("instanceLocation").GetString()).Order());
    }

    [Fact]
    public async Task LoadCase_WithOneBadScriptEntry_ReportsThatEntryAlone()
    {
        var posted = RuntimeCase();
        posted["script"]![1]!["channel"] = "radio";
        posted["script"]!.AsArray().Add(new JsonObject
        {
            ["at"] = 13000,
            ["directive"] = "heartbeatStop",
            ["channels"] = new JsonArray("trading"),
        });

        using var response = await Post(posted);
        using var refusal = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var locations = refusal.RootElement.GetProperty("errors").EnumerateArray()
            .Select(error => error.GetProperty("instanceLocation").GetString())
            .ToArray();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("/script/1/channel", locations);
        Assert.All(locations, location => Assert.StartsWith("/script/1", location, StringComparison.Ordinal));
    }

    [Fact]
    public async Task LoadCase_WithABodyThatIsNotJson_Returns422()
    {
        using var content = new StringContent("{ \"version\": ", Encoding.UTF8, "application/json");
        using var response = await _host.Client.PostAsync("/api/cases", content);
        using var refusal = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(ValidationError.Failure(refusal.RootElement));
        Assert.Single(refusal.RootElement.GetProperty("errors").EnumerateArray());
    }

    [Fact]
    public async Task LoadCase_IsNeverPersisted_AndIsGoneAfterARestart()
    {
        var filesBefore = Directory.GetFiles(SandboxHost.FixtureCases).Order().ToArray();
        var posted = RuntimeCase();
        posted["caseId"] = 100777;
        posted["tags"] = new JsonArray("runtime-restart");
        using var created = await Post(posted);

        var restarted = new SandboxHost();
        await restarted.InitializeAsync();
        try
        {
            using var afterRestart = await restarted.Client.GetAsync("/api/cases/100777");
            using var before = await _host.Client.GetAsync("/api/cases/100777");

            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            Assert.Equal(HttpStatusCode.OK, before.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, afterRestart.StatusCode);
            Assert.Equal(filesBefore, Directory.GetFiles(SandboxHost.FixtureCases).Order().ToArray());
        }
        finally
        {
            await restarted.DisposeAsync();
        }
    }

    private static JsonObject RuntimeCase() => new()
    {
        ["version"] = "0.1.0",
        ["description"] = "A case loaded at runtime.",
        ["class"] = "lifecycle",
        ["tags"] = new JsonArray("runtime-without-id"),
        ["terminal"] = "Canceled",
        ["script"] = new JsonArray(
            new JsonObject { ["at"] = 0, ["channel"] = "time_table", ["messageType"] = "time_table", ["value"] = new JsonArray() },
            new JsonObject { ["at"] = 12999, ["channel"] = "time_table", ["messageType"] = "time_table", ["value"] = new JsonArray() }),
    };

    private static int[] Ids(JsonDocument rows) =>
        rows.RootElement.EnumerateArray().Select(row => row.GetProperty("caseId").GetInt32()).ToArray();

    private static string[] Strings(JsonElement array) =>
        array.EnumerateArray().Select(item => item.GetString()).ToArray();

    private async Task<JsonDocument> GetJson(string path)
    {
        using var response = await _host.Client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> Post(JsonObject runtimeCase)
    {
        using var content = new StringContent(runtimeCase.ToJsonString(), Encoding.UTF8, "application/json");
        return await _host.Client.PostAsync("/api/cases", content);
    }
}
