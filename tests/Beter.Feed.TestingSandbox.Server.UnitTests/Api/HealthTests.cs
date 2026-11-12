using System.Net;
using System.Text.Json;
using Beter.Feed.TestingSandbox.Server.Cases;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Api;

public class HealthTests
{
    [Fact]
    public async Task GetHealth_Returns503UntilTheCaseSetIsLoaded_ThenOk()
    {
        var host = new UnloadedSandboxHost();
        await host.InitializeAsync();
        try
        {
            using var before = await host.Client.GetAsync("/api/health");
            using var error = JsonDocument.Parse(await before.Content.ReadAsStringAsync());

            host.Catalogue.Load(SandboxHost.FixtureCases);
            using var after = await host.Client.GetAsync("/api/health");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, before.StatusCode);
            Assert.Null(ContractComponent.Validator("Error").Failure(error.RootElement));
            Assert.Equal("not_ready", error.RootElement.GetProperty("error").GetString());
            Assert.Equal(HttpStatusCode.OK, after.StatusCode);
            Assert.Equal("{\"status\":\"ok\"}", await after.Content.ReadAsStringAsync());
        }
        finally
        {
            await host.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetHealth_OfAStartingHost_Answers503WhileTheCaseSetOfTheRepositoryLoads()
    {
        // The case set of the repository is over 100 MB of JSON: its load outlasts the first request.
        var host = new StartingSandboxHost();
        await host.InitializeAsync();
        try
        {
            using var first = await host.Client.GetAsync("/api/health");
            await host.Catalogue.Loaded.WaitAsync(TimeSpan.FromMinutes(2));
            using var later = await host.Client.GetAsync("/api/health");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, first.StatusCode);
            Assert.Equal(HttpStatusCode.OK, later.StatusCode);
        }
        finally
        {
            await host.DisposeAsync();
        }
    }

    [Fact]
    public async Task Host_WithACaseSetThatDoesNotLoad_Stops()
    {
        var directory = Directory.CreateTempSubdirectory("sandbox-cases-").FullName;
        await File.WriteAllTextAsync(Path.Combine(directory, "7001.json"), "{ \"caseId\": 7001 }");
        var host = new StartingSandboxHost(directory);
        await host.InitializeAsync();
        try
        {
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            var stopping = new TaskCompletionSource();
            await using var registration = lifetime.ApplicationStopping.Register(() => stopping.TrySetResult());

            await stopping.Task.WaitAsync(TimeSpan.FromSeconds(30));

            Assert.False(host.Catalogue.IsLoaded);
            Assert.Equal(1, Environment.ExitCode);
        }
        finally
        {
            // The loader marks the process as failed; this process is the test run.
            Environment.ExitCode = 0;
            await host.DisposeAsync();
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData("7001.json", "{ \"caseId\": 7001 }", "fails the case schema")]
    [InlineData("7001.json", "{ not json", "is not JSON")]
    [InlineData("7002.json", null, "named by its id")]
    [InlineData("100000.json", null, "reserved for runtime cases")]
    [InlineData("retired.json", "[{ \"caseId\": 7003 }]", "retired.json")]
    [InlineData("retired.json", "[{ \"caseId\": 7003, \"date\": \"2026-11-12\", \"replacedBy\": null }, { \"caseId\": 7003, \"date\": \"2026-11-12\", \"replacedBy\": null }]", "twice")]
    public void Load_OfACaseSetWithABadFile_FailsNamingTheFile(string name, string content, string expected)
    {
        var directory = Directory.CreateTempSubdirectory("sandbox-cases-").FullName;
        try
        {
            var valid = File.ReadAllText(Path.Combine(SandboxHost.FixtureCases, "7001.json"));
            var text = content ?? (name == "100000.json" ? valid.Replace("7001,", "100000,", StringComparison.Ordinal) : valid);
            File.WriteAllText(Path.Combine(directory, name), text);
            var catalogue = new CaseCatalogue(new CaseSchema());

            var failure = Assert.Throws<InvalidDataException>(() => catalogue.Load(directory));

            Assert.Contains(expected, failure.Message, StringComparison.Ordinal);
            Assert.False(catalogue.IsLoaded);
            Assert.True(catalogue.Loaded.IsFaulted);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Load_OfAMissingDirectory_Fails()
    {
        var catalogue = new CaseCatalogue(new CaseSchema());

        Assert.Throws<DirectoryNotFoundException>(() => catalogue.Load(Path.Combine(Path.GetTempPath(), "sandbox-no-such-cases")));
        Assert.False(catalogue.IsLoaded);
    }
}
