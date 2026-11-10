#nullable enable

using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Beter.Feed.TestingSandbox.Server.Description;

/// <summary>
/// The committed contract files, <c>openapi/sandbox.yaml</c> and <c>schemas/**</c>, as the
/// build embeds them into the host.
/// </summary>
public static class ContractResources
{
    private const string Root = "contract/";
    private const string DocumentName = Root + "openapi/sandbox.yaml";
    private const string SchemasRoot = Root + "schemas/";

    /// <summary>Loads the description as committed, references unchanged.</summary>
    /// <returns>The description.</returns>
    public static JsonObject LoadDocument()
    {
        var yaml = new YamlStream();
        using var reader = new StringReader(ReadText(DocumentName));
        yaml.Load(reader);
        return (JsonObject)ToJson(yaml.Documents[0].RootNode)!;
    }

    /// <summary>Loads the schema files, keyed by the path the description references them by.</summary>
    /// <returns>The schema files.</returns>
    public static IReadOnlyDictionary<string, JsonObject> LoadSchemas()
    {
        var assembly = typeof(ContractResources).Assembly;
        return assembly.GetManifestResourceNames()
            .Select(name => (Resource: name, Path: name.Replace('\\', '/')))
            .Where(entry => entry.Path.StartsWith(SchemasRoot, StringComparison.Ordinal))
            .ToDictionary(
                entry => string.Concat("../", entry.Path.AsSpan(Root.Length)),
                entry => (JsonObject)JsonNode.Parse(ReadText(assembly, entry.Resource))!,
                StringComparer.Ordinal);
    }

    private static string ReadText(string resource) => ReadText(typeof(ContractResources).Assembly, resource);

    private static string ReadText(Assembly assembly, string resource)
    {
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"The host does not embed '{resource}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
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
        if (scalar.Style != ScalarStyle.Plain)
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
