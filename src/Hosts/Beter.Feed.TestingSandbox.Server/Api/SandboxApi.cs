#nullable enable

using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Beter.Feed.TestingSandbox.Server.Cases;
using Beter.Feed.TestingSandbox.Server.Description;
using Beter.Feed.TestingSandbox.Server.Runs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Beter.Feed.TestingSandbox.Server.Api;

/// <summary>
/// The run half of the client-facing contract, as <c>openapi/sandbox.yaml</c> describes it.
/// The operation names, parameters and response codes declared here are compared with that
/// document by the drift check.
/// </summary>
public static class SandboxApi
{
    /// <summary>The path the description is served at.</summary>
    public const string OpenApiDocumentPath = "/openapi.json";

    /// <summary>The path the rendered description is served at.</summary>
    public const string SwaggerUiPath = "/swagger";

    /// <summary>The version of the run output schemas the sandbox writes.</summary>
    public const int OutputVersion = 1;

    private const string Json = "application/json";
    private const string JsonLines = "application/x-ndjson";

    /// <summary>Registers what the operations need.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddSandboxApi(this IServiceCollection services)
    {
        services.AddSingleton(SandboxVersion.OfThisBuild());
        services.AddSingleton<OpenApiDocumentProvider>();
        services.AddOptions<CaseOptions>().BindConfiguration(CaseOptions.SectionName);
        services.AddSingleton<CaseSchema>();
        services.AddSingleton<CaseCatalogue>();
        services.AddHostedService<CaseSetLoader>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(_ => new OffsetSequence());
        services.AddSingleton<IPlaybackSender>(provider => new HubPlaybackSender(
            provider.GetRequiredService<Publishers.IMessagePublisher<Models.TimeTableItems.TimeTableItemModel>>(),
            provider.GetRequiredService<Publishers.IMessagePublisher<Models.TradingInfos.TradingInfoModel>>(),
            provider.GetRequiredService<Publishers.IMessagePublisher<Models.Scoreboards.ScoreBoardModel>>(),
            provider.GetRequiredService<Publishers.IMessagePublisher<Models.Incidents.IncidentModel>>(),
            provider.GetRequiredService<Publishers.IFeedMessagePublisherResolver>()));
        services.AddSingleton(provider => new RunPlayer(
            provider.GetRequiredService<TimeProvider>(),
            provider.GetRequiredService<IPlaybackSender>(),
            provider.GetRequiredService<OffsetSequence>(),
            provider.GetRequiredService<ILoggerFactory>().CreateLogger<RunPlayer>()));
        services.AddSingleton(provider => new RunRegistry(
            provider.GetRequiredService<TimeProvider>(),
            provider.GetRequiredService<RunPlayer>(),
            provider.GetRequiredService<SandboxVersion>().Value));
        services.AddHostedService(provider => provider.GetRequiredService<RunRegistry>());
        services.AddOptions<HubOptions>()
            .Configure<RunRegistry>((options, runs) => options.AddFilter(new RunBindingFilter(runs)));
        return services;
    }

    /// <summary>Maps every operation of the contract.</summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The same route builder.</returns>
    public static IEndpointRouteBuilder MapSandboxApi(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api");

        api.MapGet("/version", (SandboxVersion version) => Results.Json(new VersionResponse(version.Value, OutputVersion, null)))
            .WithName("getVersion")
            .Produces<VersionResponse>(StatusCodes.Status200OK, Json);

        api.MapGet("/health", (CaseCatalogue catalogue) => catalogue.IsLoaded
                ? Results.Json(new HealthResponse("ok"))
                : Results.Json(
                    new ErrorResponse("not_ready", "The case set is not loaded yet."),
                    statusCode: StatusCodes.Status503ServiceUnavailable))
            .WithName("getHealth")
            .Produces<HealthResponse>(StatusCodes.Status200OK, Json)
            .Produces<ErrorResponse>(StatusCodes.Status503ServiceUnavailable, Json);

        MapCases(api);
        MapRuns(api);

        endpoints.MapGet(OpenApiDocumentPath, (OpenApiDocumentProvider document) => Results.Bytes(document.Json, Json))
            .WithName("getOpenApiDocument")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json);

        return endpoints;
    }

    private static void MapCases(RouteGroupBuilder api)
    {
        api.MapGet("/cases", ListCases)
            .WithName("listCases")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json)
            .Produces<JsonElement>(StatusCodes.Status400BadRequest, Json);

        api.MapPost("/cases", LoadCase)
            .WithName("loadCase")
            .Accepts<JsonElement>(Json)
            .Produces<JsonElement>(StatusCodes.Status201Created, Json)
            .Produces<JsonElement>(StatusCodes.Status422UnprocessableEntity, Json);

        api.MapGet("/cases/{caseId:int}", GetCase)
            .WithName("getCase")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json);
    }

    private static async Task<IResult> ListCases([FromQuery] string[]? tag, [FromQuery] string? status, CaseCatalogue catalogue)
    {
        var filter = status ?? "active";
        if (filter is not ("active" or "retired" or "all"))
        {
            return Results.Json(
                new ErrorResponse("invalid_status", "status is one of active, retired, all."),
                statusCode: StatusCodes.Status400BadRequest);
        }

        await catalogue.Loaded;
        var entries = catalogue.List(tag ?? Array.Empty<string>(), active: filter != "retired", retired: filter != "active");
        return Results.Json(new JsonArray(entries.Select(entry => (JsonNode?)CatalogueRow(entry)).ToArray()));
    }

    private static async Task<IResult> LoadCase(HttpRequest request, CaseCatalogue catalogue)
    {
        await catalogue.Loaded;

        JsonDocument posted;
        try
        {
            posted = await JsonDocument.ParseAsync(request.Body, cancellationToken: request.HttpContext.RequestAborted);
        }
        catch (JsonException exception)
        {
            return InvalidCase(new[] { new SchemaError(string.Empty, string.Empty, $"The body is not JSON: {exception.Message}") });
        }

        using (posted)
        {
            var result = catalogue.AddRuntimeCase(posted.RootElement);
            return result.Entry is null
                ? InvalidCase(result.Errors)
                : Results.Json(CatalogueRow(result.Entry), statusCode: StatusCodes.Status201Created);
        }
    }

    private static async Task<IResult> GetCase(int caseId, CaseCatalogue catalogue, CancellationToken cancellationToken)
    {
        await catalogue.Loaded;
        var entry = catalogue.Find(caseId);
        if (entry is null)
        {
            return Results.Json(
                new ErrorResponse("not_found", $"No case has the id {caseId}."),
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Stream(body => WriteCaseDetailAsync(body, entry, cancellationToken), Json);
    }

    private static IResult InvalidCase(IReadOnlyList<SchemaError> errors) => Results.Json(
        new ValidationErrorResponse(
            "invalid_case",
            "The case fails the case schema, or its id is in use or outside the runtime range.",
            errors),
        statusCode: StatusCodes.Status422UnprocessableEntity);

    /// <summary>The catalogue row of an entry: <c>CatalogueRow</c> of the description.</summary>
    private static JsonObject CatalogueRow(CaseEntry entry)
    {
        var row = new JsonObject { ["caseId"] = entry.CaseId };
        var summary = entry.Summary;
        if (summary is not null)
        {
            row["version"] = summary.Version;
        }

        row["status"] = entry.Retired ? "retired" : "active";
        if (summary is not null)
        {
            row["class"] = summary.Class;
            if (summary.Sport is { } sport)
            {
                row["sport"] = sport;
            }

            row["tags"] = new JsonArray(summary.Tags.Select(tag => (JsonNode?)tag).ToArray());
            row["description"] = summary.Description;
            row["terminal"] = summary.Terminal;
            row["covers"] = new JsonObject
            {
                ["count"] = summary.Covers.Count,
                ["sections"] = new JsonArray(summary.Covers
                    .Distinct()
                    .Select(section => (JsonNode?)new JsonObject { ["page"] = section.Page, ["section"] = section.Section })
                    .ToArray()),
            };
            row["duration"] = summary.Duration;
        }

        if (entry.Retired)
        {
            row["replacedBy"] = entry.ReplacedBy;
        }

        return row;
    }

    /// <summary>
    /// Writes <c>CaseDetail</c> of the description: the case file with its status, or the id,
    /// status and replacement alone for a retired id that has no case file. The case file is
    /// copied as it is, up to its closing brace, so a file of any size costs one buffer.
    /// </summary>
    private static async Task WriteCaseDetailAsync(Stream body, CaseEntry entry, CancellationToken cancellationToken)
    {
        var status = entry.Retired
            ? $"\"status\":\"retired\",\"replacedBy\":{entry.ReplacedBy?.ToString(CultureInfo.InvariantCulture) ?? "null"}}}"
            : "\"status\":\"active\"}";

        await using var caseFile = entry.OpenFile();
        if (caseFile is null)
        {
            var bare = string.Create(CultureInfo.InvariantCulture, $"{{\"caseId\":{entry.CaseId},{status}");
            await body.WriteAsync(Encoding.UTF8.GetBytes(bare), cancellationToken);
            return;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            var remaining = await ClosingBraceOfAsync(caseFile, buffer, cancellationToken);
            caseFile.Position = 0;
            while (remaining > 0)
            {
                var read = await caseFile.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)), cancellationToken);
                if (read == 0)
                {
                    throw new InvalidDataException($"The case file of {entry.CaseId} ended early.");
                }

                await body.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                remaining -= read;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        await body.WriteAsync(Encoding.UTF8.GetBytes("," + status), cancellationToken);
    }

    /// <summary>The offset of the closing brace of a case file: a JSON object, as its load validated.</summary>
    private static async Task<long> ClosingBraceOfAsync(Stream caseFile, byte[] buffer, CancellationToken cancellationToken)
    {
        var tail = (int)Math.Min(caseFile.Length, 256);
        caseFile.Position = caseFile.Length - tail;
        await caseFile.ReadExactlyAsync(buffer.AsMemory(0, tail), cancellationToken);
        var brace = buffer.AsSpan(0, tail).LastIndexOf((byte)'}');
        return brace < 0
            ? throw new InvalidDataException("The case file does not end in an object.")
            : caseFile.Length - tail + brace;
    }

    private static void MapRuns(RouteGroupBuilder api)
    {
        api.MapPost("/runs", StartRun)
            .WithName("startRun")
            .Accepts<JsonElement>(Json)
            .Produces<JsonElement>(StatusCodes.Status201Created, Json)
            .Produces<JsonElement>(StatusCodes.Status400BadRequest, Json)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json)
            .Produces<JsonElement>(StatusCodes.Status410Gone, Json);

        api.MapGet("/runs", ListRuns)
            .WithName("listRuns")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json)
            .Produces<JsonElement>(StatusCodes.Status400BadRequest, Json);

        api.MapGet("/runs/{runId}", GetRun)
            .WithName("getRun")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json);

        api.MapDelete("/runs/{runId}", DeleteRun)
            .WithName("deleteRun")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json)
            .Produces<JsonElement>(StatusCodes.Status409Conflict, Json);

        api.MapGet("/runs/{runId}/messages", (string runId) => NotImplemented())
            .WithName("getRunMessages")
            .Produces<JsonElement>(StatusCodes.Status200OK, JsonLines)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json);

        api.MapGet("/runs/{runId}/events", (string runId) => NotImplemented())
            .WithName("getRunEvents")
            .Produces<JsonElement>(StatusCodes.Status200OK, JsonLines)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json);

        api.MapPost("/runs/{runId}/stop", StopRun)
            .WithName("stopRun")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json)
            .Produces<JsonElement>(StatusCodes.Status409Conflict, Json);
    }

    private static async Task<IResult> StartRun(HttpRequest request, CaseCatalogue catalogue, RunRegistry runs)
    {
        await catalogue.Loaded;

        RunRequest? posted;
        ErrorResponse? refused;
        try
        {
            using var body = await JsonDocument.ParseAsync(request.Body, cancellationToken: request.HttpContext.RequestAborted);
            posted = RunRequest.Read(body.RootElement, out refused);
        }
        catch (JsonException exception)
        {
            posted = null;
            refused = new ErrorResponse("invalid_body", $"The body is not JSON: {exception.Message}");
        }

        if (posted is null)
        {
            return Results.Json(refused, statusCode: StatusCodes.Status400BadRequest);
        }

        var entry = catalogue.Find(posted.CaseId);
        if (entry is null)
        {
            return Results.Json(
                new ErrorResponse("not_found", $"No case has the id {posted.CaseId}."),
                statusCode: StatusCodes.Status404NotFound);
        }

        if (entry.Retired || entry.Summary is null)
        {
            return Results.Json(
                new RetiredErrorResponse("retired", $"Case {posted.CaseId} is retired.", entry.ReplacedBy),
                statusCode: StatusCodes.Status410Gone);
        }

        var acceleration = posted.Acceleration ?? RunRequest.DefaultAcceleration;
        if (acceleration < RunRequest.MinAcceleration || acceleration > entry.Summary.MaxAcceleration)
        {
            return Results.Json(
                new ErrorResponse(
                    "invalid_acceleration",
                    string.Create(
                        CultureInfo.InvariantCulture,
                        $"acceleration is from {RunRequest.MinAcceleration} to {entry.Summary.MaxAcceleration}, the maxAcceleration of case {posted.CaseId}.")),
                statusCode: StatusCodes.Status400BadRequest);
        }

        var run = runs.Create(
            entry,
            posted.ApiKey,
            acceleration,
            posted.Start ?? entry.Summary.Start ?? RunStart.OnConnect,
            TimeSpan.FromSeconds(posted.WaitTimeoutSeconds));
        return Results.Json(run.ToManifest(), statusCode: StatusCodes.Status201Created);
    }

    private static IResult ListRuns([FromQuery] string? status, RunRegistry runs)
    {
        if (status is not null && !RunStatus.IsKnown(status))
        {
            return Results.Json(
                new ErrorResponse("invalid_status", $"status is one of {string.Join(", ", RunStatus.All)}."),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Json(new JsonArray(runs.List(status).Select(run => (JsonNode?)run.ToManifest()).ToArray()));
    }

    private static IResult GetRun(string runId, RunRegistry runs) =>
        runs.Find(runId) is { } run ? Results.Json(run.ToManifest()) : UnknownRun(runId);

    private static IResult StopRun(string runId, RunRegistry runs) => runs.Stop(runId, out var run) switch
    {
        RunOutcome.Done => Results.Json(run!.ToManifest()),
        RunOutcome.Conflict => Results.Json(
            new ErrorResponse("run_finished", $"Run {runId} is {run!.Status} already."),
            statusCode: StatusCodes.Status409Conflict),
        _ => UnknownRun(runId),
    };

    private static IResult DeleteRun(string runId, RunRegistry runs) => runs.Delete(runId) switch
    {
        RunOutcome.Done => Results.NoContent(),
        RunOutcome.Conflict => Results.Json(
            new ErrorResponse("run_live", $"Run {runId} is waiting or playing; stop it first."),
            statusCode: StatusCodes.Status409Conflict),
        _ => UnknownRun(runId),
    };

    private static IResult UnknownRun(string runId) => Results.Json(
        new ErrorResponse("not_found", $"No run has the id {runId}."),
        statusCode: StatusCodes.Status404NotFound);

    private static IResult NotImplemented() => Results.Json(
        new ErrorResponse("not_implemented", "This version of the sandbox does not implement the operation yet."),
        statusCode: StatusCodes.Status501NotImplemented);
}
