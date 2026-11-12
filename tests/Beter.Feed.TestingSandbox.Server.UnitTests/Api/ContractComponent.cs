using System.Text.Json;
using System.Text.Json.Nodes;
using Beter.Feed.TestingSandbox.Server.Cases;
using Beter.Feed.TestingSandbox.Server.Description;
using LateApexEarlySpeed.Json.Schema;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Api;

/// <summary>
/// The schemas of <c>components.schemas</c> of the committed description, as validators of
/// what the host answers.
/// </summary>
internal static class ContractComponent
{
    private const string CaseSchemaDefinitions = CaseSchema.SchemaPath + "#/$defs/";

    /// <summary>A validator of one component, with the case schema definitions it references.</summary>
    public static JsonValidator Validator(string component)
    {
        var definitions = ContractResources.LoadSchemas()[CaseSchema.SchemaPath]["$defs"]!.DeepClone().AsObject();
        foreach (var (name, schema) in ContractResources.LoadDocument()["components"]!["schemas"]!.AsObject())
        {
            var isFileAlias = schema is JsonObject { Count: 1 } alias
                && alias["$ref"] is JsonValue file
                && !file.GetValue<string>().Contains('#');
            if (!isFileAlias)
            {
                definitions[name] = WithLocalReferences(schema!.DeepClone());
            }
        }

        var bundle = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["$ref"] = $"#/$defs/{component}",
            ["$defs"] = definitions,
        };
        return new JsonValidator(bundle.ToJsonString());
    }

    /// <summary>The failure of an instance against a component, or null for a valid instance.</summary>
    public static string Failure(this JsonValidator validator, JsonElement instance)
    {
        var result = validator.Validate(instance);
        return result.IsValid
            ? null
            : string.Join("; ", result.ValidationErrors.Select(error =>
                $"{error.Keyword} at '{error.InstanceLocation}': {error.ErrorMessage}"));
    }

    private static JsonNode WithLocalReferences(JsonNode node)
    {
        switch (node)
        {
            case JsonObject json:
                foreach (var (key, value) in json.ToArray())
                {
                    if (key == "$ref" && value is JsonValue reference)
                    {
                        json[key] = reference.GetValue<string>()
                            .Replace(CaseSchemaDefinitions, "#/$defs/", StringComparison.Ordinal)
                            .Replace("#/components/schemas/", "#/$defs/", StringComparison.Ordinal);
                    }
                    else if (value is not null)
                    {
                        WithLocalReferences(value);
                    }
                }

                break;
            case JsonArray array:
                foreach (var item in array.Where(item => item is not null))
                {
                    WithLocalReferences(item);
                }

                break;
        }

        return node;
    }
}
