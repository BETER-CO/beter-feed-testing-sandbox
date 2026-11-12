#nullable enable

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Beter.Feed.TestingSandbox.Server.Description;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>
/// The case schema the build embeds, <c>schemas/case/case.schema.json</c>, in its two uses:
/// a case file of the repository, and a case posted at runtime.
/// </summary>
public sealed partial class CaseSchema
{
    /// <summary>The path the description references the case schema by.</summary>
    public const string SchemaPath = "../schemas/case/case.schema.json";

    /// <summary>The keyword location of <c>caseId</c>, as a validation of the case schema reports it.</summary>
    public const string CaseIdKeyword = "/$ref/properties/caseId";

    // The runtime range is this class's own keyword; its failures are reported at the
    // caseId of the case schema, whose description states the range.
    private const string RuntimeRangeKeyword = "/properties/caseId";

    private const string Alternatives = "/oneOf";

    private static readonly JsonSchemaOptions EveryError = new() { OutputFormat = OutputFormat.List };

    private readonly JsonValidator _caseFile;
    private readonly JsonValidator _runtimeCase;

    /// <summary>Initializes a new instance of the <see cref="CaseSchema"/> class.</summary>
    public CaseSchema()
    {
        var schema = ContractResources.LoadSchemas()[SchemaPath];
        _caseFile = new JsonValidator(schema.ToJsonString());

        // The posted form, RuntimeCase of the description: no id, or one of the runtime range.
        var runtime = schema.DeepClone().AsObject();
        runtime.Remove("required");
        runtime["properties"] = new JsonObject
        {
            ["caseId"] = new JsonObject { ["minimum"] = CaseCatalogue.FirstRuntimeId },
        };
        _runtimeCase = new JsonValidator(runtime.ToJsonString());
    }

    /// <summary>Validates a case file of the repository.</summary>
    /// <param name="caseFile">The case file.</param>
    /// <returns>The failed keywords; empty for a valid file.</returns>
    public IReadOnlyList<SchemaError> ValidateCaseFile(JsonElement caseFile) => Validate(_caseFile, caseFile);

    /// <summary>Validates a case posted at runtime.</summary>
    /// <param name="runtimeCase">The posted case.</param>
    /// <returns>The failed keywords; empty for a valid case.</returns>
    public IReadOnlyList<SchemaError> ValidateRuntimeCase(JsonElement runtimeCase) =>
        Validate(_runtimeCase, runtimeCase)
            .Select(error => error.KeywordLocation.StartsWith(RuntimeRangeKeyword, StringComparison.Ordinal)
                ? error with { KeywordLocation = CaseIdKeyword }
                : error)
            .ToArray();

    private static SchemaError[] Validate(JsonValidator validator, JsonElement instance)
    {
        var result = validator.Validate(instance, EveryError);
        if (result.IsValid)
        {
            return [];
        }

        var errors = result.ValidationErrors
            .Select(error => new SchemaError(
                error.InstanceLocation?.ToString() ?? string.Empty,
                error.RelativeKeywordLocation?.ToString() ?? string.Empty,
                error.ErrorMessage ?? error.Keyword ?? "The case fails the case schema."))
            .ToArray();
        return errors.Where(error => IsAFailure(error, errors)).ToArray();
    }

    /// <summary>
    /// Whether a listed error is a failure of the instance. The list also reports the keywords
    /// that failed on the way to a valid result: the condition of an <c>if</c>, and the
    /// alternatives of a <c>oneOf</c> an entry did not take. An error inside an alternative
    /// counts only where the <c>oneOf</c> itself failed for that part of the instance.
    /// </summary>
    private static bool IsAFailure(SchemaError error, SchemaError[] errors)
    {
        if (error.KeywordLocation.Contains("/if/", StringComparison.Ordinal))
        {
            return false;
        }

        foreach (Match alternative in Alternative().Matches(error.KeywordLocation))
        {
            var oneOf = error.KeywordLocation[..(alternative.Index + Alternatives.Length)];
            var failed = errors.Any(other =>
                string.Equals(other.KeywordLocation, oneOf, StringComparison.Ordinal)
                && (string.Equals(error.InstanceLocation, other.InstanceLocation, StringComparison.Ordinal)
                    || error.InstanceLocation.StartsWith(other.InstanceLocation + "/", StringComparison.Ordinal)));
            if (!failed)
            {
                return false;
            }
        }

        return true;
    }

    [GeneratedRegex("/oneOf/[0-9]+/")]
    private static partial Regex Alternative();
}
