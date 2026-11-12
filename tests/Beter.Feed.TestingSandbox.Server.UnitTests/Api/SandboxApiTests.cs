using System.Net;
using System.Text.Json;
using Beter.Feed.TestingSandbox.Server.Api;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Api;

public class SandboxApiTests : IClassFixture<SandboxHost>
{
    private readonly SandboxHost _host;

    public SandboxApiTests(SandboxHost host)
    {
        _host = host;
    }

    public static TheoryData<string, string> OperationsNotBuiltYet => new()
    {
        { "POST", "/api/runs" },
        { "GET", "/api/runs" },
        { "GET", "/api/runs/run-1" },
        { "DELETE", "/api/runs/run-1" },
        { "GET", "/api/runs/run-1/messages" },
        { "GET", "/api/runs/run-1/events" },
        { "POST", "/api/runs/run-1/stop" },
    };

    [Fact]
    public async Task GetVersion_ReturnsTheVersionOfTheBuild()
    {
        using var response = await _host.Client.GetAsync("/api/version");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(SandboxVersion.OfThisBuild().Value, body.RootElement.GetProperty("sandboxVersion").GetString());
        Assert.Equal(SandboxApi.OutputVersion, body.RootElement.GetProperty("outputVersion").GetInt32());
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("contract").ValueKind);
    }

    [Fact]
    public async Task GetHealth_ReturnsOkOnceTheCaseSetIsLoaded()
    {
        using var response = await _host.Client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"status\":\"ok\"}", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [MemberData(nameof(OperationsNotBuiltYet))]
    public async Task OperationNotBuiltYet_Returns501WithAnErrorBody(string method, string path)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        using var response = await _host.Client.SendAsync(request);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
        Assert.Equal("not_implemented", body.RootElement.GetProperty("error").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty("detail").GetString()));
    }

    [Fact]
    public async Task GetOpenApiDocument_ServesTheDescriptionWithTheVersionOfTheBuild()
    {
        using var response = await _host.Client.GetAsync(SandboxApi.OpenApiDocumentPath);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("3.1.0", body.RootElement.GetProperty("openapi").GetString());
        Assert.Equal(
            SandboxVersion.OfThisBuild().Value,
            body.RootElement.GetProperty("info").GetProperty("version").GetString());
    }

    [Fact]
    public async Task GetSwagger_RendersTheServedDescription()
    {
        using var page = await _host.Client.GetAsync($"{SandboxApi.SwaggerUiPath}/index.html");
        using var redirect = await _host.Client.GetAsync(SandboxApi.SwaggerUiPath);
        using var settings = await _host.Client.GetAsync($"{SandboxApi.SwaggerUiPath}/index.js");
        var script = await settings.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Equal("text/html", page.Content.Headers.ContentType?.MediaType);
        Assert.True(
            redirect.StatusCode is HttpStatusCode.OK or HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect,
            $"GET {SandboxApi.SwaggerUiPath} answered {(int)redirect.StatusCode}.");
        Assert.Contains(SandboxApi.OpenApiDocumentPath, script, StringComparison.Ordinal);
    }
}
