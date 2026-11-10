#nullable enable

using System.Text.Json;
using Beter.Feed.TestingSandbox.Server.Description;
using Microsoft.AspNetCore.Mvc;

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

        api.MapGet("/health", () => Results.Json(new HealthResponse("ok")))
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
        api.MapGet("/cases", ([FromQuery] string[]? tag, [FromQuery] string? status) => NotImplemented())
            .WithName("listCases")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json);

        api.MapPost("/cases", NotImplemented)
            .WithName("loadCase")
            .Accepts<JsonElement>(Json)
            .Produces<JsonElement>(StatusCodes.Status201Created, Json)
            .Produces<JsonElement>(StatusCodes.Status422UnprocessableEntity, Json);

        api.MapGet("/cases/{caseId:int}", (int caseId) => NotImplemented())
            .WithName("getCase")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json);
    }

    private static void MapRuns(RouteGroupBuilder api)
    {
        api.MapPost("/runs", NotImplemented)
            .WithName("startRun")
            .Accepts<JsonElement>(Json)
            .Produces<JsonElement>(StatusCodes.Status201Created, Json)
            .Produces<JsonElement>(StatusCodes.Status400BadRequest, Json)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json)
            .Produces<JsonElement>(StatusCodes.Status410Gone, Json);

        api.MapGet("/runs", ([FromQuery] string? status) => NotImplemented())
            .WithName("listRuns")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json);

        api.MapGet("/runs/{runId}", (string runId) => NotImplemented())
            .WithName("getRun")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json);

        api.MapDelete("/runs/{runId}", (string runId) => NotImplemented())
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

        api.MapPost("/runs/{runId}/stop", (string runId) => NotImplemented())
            .WithName("stopRun")
            .Produces<JsonElement>(StatusCodes.Status200OK, Json)
            .Produces<JsonElement>(StatusCodes.Status404NotFound, Json)
            .Produces<JsonElement>(StatusCodes.Status409Conflict, Json);
    }

    private static IResult NotImplemented() => Results.Json(
        new ErrorResponse("not_implemented", "This version of the sandbox does not implement the operation yet."),
        statusCode: StatusCodes.Status501NotImplemented);
}
