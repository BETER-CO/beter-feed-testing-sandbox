using System.Globalization;
using System.Text.Json.Nodes;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using YamlDotNet.RepresentationModel;

namespace Beter.Feed.TestingSandbox.ContractTests;

/// <summary>
/// The committed contract files, as the build copies them beside the tests.
/// </summary>
internal static class ContractFiles
{
    public const string OpenApiDocument = "openapi/sandbox.yaml";
    public const string CaseSchema = "schemas/case/case.schema.json";
    public const string RunSchema = "schemas/run-output/v1/run.schema.json";
    public const string MessageSchema = "schemas/run-output/v1/message.schema.json";
    public const string EventSchema = "schemas/run-output/v1/event.schema.json";

    private static readonly string ContractRoot = Path.Combine(AppContext.BaseDirectory, "contract");

    public static string PathOf(string relativePath) =>
        Path.GetFullPath(Path.Combine(ContractRoot, relativePath));

    public static string RelativePathOf(string fullPath) =>
        Path.GetRelativePath(ContractRoot, fullPath).Replace('\\', '/');

    public static string ReadText(string relativePath) => File.ReadAllText(PathOf(relativePath));

    public static string ReadResource(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", name));

    /// <summary>
    /// Loads a JSON or a YAML file as a JSON node.
    /// </summary>
    public static JsonNode Load(string relativePath) => LoadFile(PathOf(relativePath));

    public static JsonNode LoadFile(string fullPath)
    {
        var text = File.ReadAllText(fullPath);
        if (!fullPath.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
        {
            return JsonNode.Parse(text)!;
        }

        var yaml = new YamlStream();
        using var reader = new StringReader(text);
        yaml.Load(reader);
        return ToJson(yaml.Documents[0].RootNode)!;
    }

    public static JsonValidator ValidatorOf(string schemaPath) => new(ReadText(schemaPath));

    /// <summary>
    /// A validator of one schema of the OpenAPI document's components, with the
    /// case schema definitions it references brought into one schema.
    /// </summary>
    public static JsonValidator ComponentValidatorOf(string component)
    {
        var definitions = Load(CaseSchema)["$defs"]!.DeepClone().AsObject();
        foreach (var (name, schema) in Load(OpenApiDocument)["components"]!["schemas"]!.AsObject())
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
                            .Replace("../schemas/case/case.schema.json#/$defs/", "#/$defs/")
                            .Replace("#/components/schemas/", "#/$defs/");
                    }
                    else if (value is not null)
                    {
                        WithLocalReferences(value);
                    }
                }

                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    if (item is not null)
                    {
                        WithLocalReferences(item);
                    }
                }

                break;
        }

        return node;
    }

    /// <summary>
    /// Validates an instance and returns the failure, or null for a valid instance.
    /// </summary>
    public static string? Failure(this JsonValidator validator, JsonNode instance)
    {
        ValidationResult result = validator.Validate(instance.ToJsonString());
        return result.IsValid
            ? null
            : string.Join("; ", result.ValidationErrors.Select(error =>
                $"{error.Keyword} at '{error.InstanceLocation}': {error.ErrorMessage}"));
    }

    private static JsonNode? ToJson(YamlNode node) => node switch
    {
        YamlMappingNode mapping => new JsonObject(mapping.Children.Select(pair =>
            KeyValuePair.Create(((YamlScalarNode)pair.Key).Value!, ToJson(pair.Value)))),
        YamlSequenceNode sequence => new JsonArray(sequence.Children.Select(ToJson).ToArray()),
        YamlScalarNode scalar => ToJson(scalar),
        _ => throw new NotSupportedException($"YAML node {node.NodeType} is not used by the contract."),
    };

    private static JsonNode? ToJson(YamlScalarNode scalar)
    {
        var value = scalar.Value ?? string.Empty;
        if (scalar.Style != YamlDotNet.Core.ScalarStyle.Plain)
        {
            return JsonValue.Create(value);
        }

        return value switch
        {
            "" or "~" or "null" => null,
            "true" => JsonValue.Create(true),
            "false" => JsonValue.Create(false),
            _ when long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var integer) =>
                JsonValue.Create(integer),
            _ when double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) =>
                JsonValue.Create(number),
            _ => JsonValue.Create(value),
        };
    }
}
