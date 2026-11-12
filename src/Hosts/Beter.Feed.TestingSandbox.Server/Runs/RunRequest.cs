#nullable enable

using System.Text.Json;
using Beter.Feed.TestingSandbox.Server.Api;

namespace Beter.Feed.TestingSandbox.Server.Runs;

/// <summary>
/// The body of <c>POST /api/runs</c>, <c>RunRequest</c> of the description, with the checks
/// that need no case. A value the client left out is null here.
/// </summary>
/// <param name="CaseId">The case to play.</param>
/// <param name="ApiKey">The value that binds the run, compared as a GUID.</param>
/// <param name="Acceleration">The acceleration; null for the default of 1.</param>
/// <param name="Start">The start mode; null for the case's own.</param>
/// <param name="WaitTimeoutSeconds">How long an <c>onConnect</c> run waits for a first connection.</param>
internal sealed record RunRequest(int CaseId, Guid ApiKey, double? Acceleration, string? Start, int WaitTimeoutSeconds)
{
    /// <summary>The acceleration of a request that names none.</summary>
    public const double DefaultAcceleration = 1;

    /// <summary>The lowest acceleration a run accepts.</summary>
    public const double MinAcceleration = 0.25;

    /// <summary>The wait of a request that names none, in seconds.</summary>
    public const int DefaultWaitTimeoutSeconds = 60;

    private static readonly string[] Properties = ["caseId", "apiKey", "acceleration", "start", "waitTimeoutSeconds"];

    /// <summary>
    /// Reads a posted body, checking in the order of the description: the shape, <c>caseId</c>,
    /// <c>apiKey</c>, unknown properties, the key's form, <c>start</c>,
    /// <c>waitTimeoutSeconds</c> and the type of <c>acceleration</c>.
    /// </summary>
    /// <param name="body">The posted body.</param>
    /// <param name="error">Why the body is refused; null for a body that is read.</param>
    /// <returns>The request, or null for a refused body.</returns>
    public static RunRequest? Read(JsonElement body, out ErrorResponse? error)
    {
        error = Check(body);
        if (error is not null)
        {
            return null;
        }

        var start = body.TryGetProperty("start", out var startValue) ? startValue.GetString() : null;
        var wait = body.TryGetProperty("waitTimeoutSeconds", out var waitValue) ? waitValue.GetInt32() : DefaultWaitTimeoutSeconds;
        double? acceleration = body.TryGetProperty("acceleration", out var accelerationValue) ? accelerationValue.GetDouble() : null;
        return new RunRequest(
            body.GetProperty("caseId").GetInt32(),
            Guid.ParseExact(body.GetProperty("apiKey").GetString()!, "D"),
            acceleration,
            start,
            wait);
    }

    private static ErrorResponse? Check(JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            return Invalid("invalid_body", "The body is a JSON object, RunRequest of the description.");
        }

        if (!body.TryGetProperty("caseId", out var caseId) || caseId.ValueKind != JsonValueKind.Number
            || !caseId.TryGetInt32(out var id) || id < 1)
        {
            return Invalid("invalid_case_id", "caseId is required and is a whole number from 1 up.");
        }

        if (!body.TryGetProperty("apiKey", out var apiKey) || apiKey.ValueKind != JsonValueKind.String)
        {
            return Invalid("invalid_api_key", "apiKey is required and is a string: a GUID.");
        }

        var unknown = body.EnumerateObject().FirstOrDefault(property => !Properties.Contains(property.Name, StringComparer.Ordinal));
        if (unknown.Value.ValueKind != JsonValueKind.Undefined)
        {
            return Invalid("unknown_property", $"RunRequest has no property '{unknown.Name}'.");
        }

        if (!Guid.TryParseExact(apiKey.GetString(), "D", out var key) || key == Guid.Empty)
        {
            return Invalid(
                "invalid_api_key",
                "apiKey is a non-empty GUID in the 8-4-4-4-12 form with hyphens, the feed ApiKey the hubs accept.");
        }

        if (body.TryGetProperty("start", out var start)
            && (start.ValueKind != JsonValueKind.String || start.GetString() is not (RunStart.OnConnect or RunStart.Now)))
        {
            return Invalid("invalid_start", "start is one of onConnect, now.");
        }

        if (body.TryGetProperty("waitTimeoutSeconds", out var wait)
            && (wait.ValueKind != JsonValueKind.Number || !wait.TryGetInt32(out var seconds) || seconds < 1))
        {
            return Invalid("invalid_wait_timeout", "waitTimeoutSeconds is a whole number from 1 up.");
        }

        if (body.TryGetProperty("acceleration", out var acceleration) && acceleration.ValueKind != JsonValueKind.Number)
        {
            return Invalid("invalid_acceleration", "acceleration is a number.");
        }

        return null;
    }

    private static ErrorResponse Invalid(string error, string detail) => new(error, detail);
}
