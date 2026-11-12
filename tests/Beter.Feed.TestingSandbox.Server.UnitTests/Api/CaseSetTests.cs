using System.Net;
using System.Text.Json;
using Beter.Feed.TestingSandbox.Server.Cases;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using LateApexEarlySpeed.Json.Schema;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Api;

/// <summary>
/// The case set of the repository, <c>cases/</c>, loaded as the image loads it. Loading
/// validates every case file against the case schema.
/// </summary>
public class CaseSetTests : IClassFixture<ImageCaseSetHost>
{
    private static readonly JsonValidator CatalogueRow = ContractComponent.Validator("CatalogueRow");
    private static readonly JsonValidator CaseDetail = ContractComponent.Validator("CaseDetail");

    private readonly ImageCaseSetHost _host;

    public CaseSetTests(ImageCaseSetHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task SmokeCases_AreListedWithEveryCatalogueField()
    {
        using var response = await _host.Client.GetAsync("/api/cases?tag=smoke&status=all");
        using var rows = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var fields = new[] { "caseId", "version", "status", "class", "sport", "tags", "description", "terminal", "covers", "duration" };

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            new[] { 1001, 1002, 1022, 1023, 2001, 2002 },
            rows.RootElement.EnumerateArray().Select(row => row.GetProperty("caseId").GetInt32()));
        Assert.All(rows.RootElement.EnumerateArray(), row =>
        {
            Assert.Null(CatalogueRow.Failure(row));
            Assert.All(fields, field => Assert.True(row.TryGetProperty(field, out _), $"The row has no {field}."));
            Assert.Equal("baseline", row.GetProperty("class").GetString());
            Assert.Equal(0, row.GetProperty("covers").GetProperty("count").GetInt32());
            Assert.True(row.GetProperty("duration").GetInt32() > 0);
        });
    }

    [Fact]
    public async Task CaseSet_HasTheConvertedCasesAndTheRetiredIds()
    {
        using var response = await _host.Client.GetAsync("/api/cases?status=all");
        using var rows = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var retired = rows.RootElement.EnumerateArray()
            .Where(row => row.GetProperty("status").GetString() == "retired")
            .ToArray();

        Assert.All(rows.RootElement.EnumerateArray(), row => Assert.Null(CatalogueRow.Failure(row)));
        Assert.Equal(24, rows.RootElement.GetArrayLength() - retired.Length);
        Assert.Equal(new[] { 1009, 1010, 1014 }, retired.Select(row => row.GetProperty("caseId").GetInt32()));
        Assert.All(retired, row => Assert.Equal(JsonValueKind.Null, row.GetProperty("replacedBy").ValueKind));
    }

    [Fact]
    public async Task RetiredId_ResolvesWithItsStatusAndReplacement()
    {
        using var response = await _host.Client.GetAsync("/api/cases/1014");
        var body = await response.Content.ReadAsStringAsync();
        using var served = JsonDocument.Parse(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(CaseDetail.Failure(served.RootElement));
        Assert.Equal("""{"caseId":1014,"status":"retired","replacedBy":null}""", body);
    }

    [Fact]
    public async Task ConvertedCase_IsServedWithItsScript()
    {
        using var response = await _host.Client.GetAsync("/api/cases/5");
        using var served = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        var directives = served.RootElement.GetProperty("script").EnumerateArray()
            .Where(entry => entry.TryGetProperty("directive", out _))
            .Select(entry => (entry.GetProperty("directive").GetString(), entry.GetProperty("at").GetInt32()))
            .ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("active", served.RootElement.GetProperty("status").GetString());
        Assert.Equal("transport-faults", served.RootElement.GetProperty("class").GetString());
        Assert.Equal(new[] { ("heartbeatStop", 228120), ("heartbeatResume", 248120) }, directives);
    }

    [Fact]
    public void CaseFiles_AreNamedByTheirId_AndStayBelowTheRuntimeRange()
    {
        var directory = CaseSetLoader.DirectoryOf(new CaseOptions());
        var names = Directory.GetFiles(directory, "*.json").Select(Path.GetFileNameWithoutExtension).ToArray();

        Assert.Contains("retired", names);
        Assert.All(names.Where(name => name != "retired"), name => Assert.InRange(int.Parse(name), 1, CaseCatalogue.FirstRuntimeId - 1));
    }
}
