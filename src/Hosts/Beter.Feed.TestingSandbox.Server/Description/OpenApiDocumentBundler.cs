#nullable enable

using System.Globalization;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Server.Description;

/// <summary>
/// Turns the committed description, which references the JSON Schema files by relative
/// path, into one self-contained document: every referenced file becomes a schema of
/// <c>components/schemas</c> and every reference becomes a local one.
/// </summary>
public static class OpenApiDocumentBundler
{
    private const string Reference = "$ref";
    private const string ComponentPrefix = "#/components/schemas/";
    private const string SchemaFileSuffix = ".schema.json";

    /// <summary>Bundles a description with the schema files it references.</summary>
    /// <param name="document">The description, as committed.</param>
    /// <param name="files">The schema files, keyed by the relative path the description uses.</param>
    /// <returns>A new document with no reference outside itself.</returns>
    public static JsonObject Bundle(JsonObject document, IReadOnlyDictionary<string, JsonObject> files)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(files);

        var bundle = (JsonObject)document.DeepClone();
        var schemas = bundle["components"]?["schemas"] as JsonObject
            ?? throw new InvalidOperationException("The description has no components/schemas.");

        var names = NamesOfReferencedFiles(bundle, schemas, files);

        Rewrite(bundle, reference => ToLocal(reference, names));

        foreach (var (file, name) in names)
        {
            var embedded = (JsonObject)files[file].DeepClone();
            embedded.Remove("$schema");
            Rewrite(embedded, reference => reference.StartsWith('#')
                ? string.Concat(ComponentPrefix, name, reference.AsSpan(1))
                : throw new InvalidOperationException($"{file} references '{reference}' outside itself."));
            schemas[name] = embedded;
        }

        return bundle;
    }

    /// <summary>Lists the value of every reference of a node, in document order.</summary>
    /// <param name="node">The node to walk.</param>
    /// <returns>The references.</returns>
    public static IEnumerable<string> ReferencesOf(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject json:
                foreach (var (key, value) in json)
                {
                    if (key == Reference && value is JsonValue reference)
                    {
                        yield return reference.GetValue<string>();
                    }
                    else
                    {
                        foreach (var nested in ReferencesOf(value))
                        {
                            yield return nested;
                        }
                    }
                }

                break;
            case JsonArray array:
                foreach (var nested in array.SelectMany(ReferencesOf))
                {
                    yield return nested;
                }

                break;
        }
    }

    private static Dictionary<string, string> NamesOfReferencedFiles(
        JsonObject bundle,
        JsonObject schemas,
        IReadOnlyDictionary<string, JsonObject> files)
    {
        var referenced = ReferencesOf(bundle)
            .Where(reference => !reference.StartsWith('#'))
            .Select(reference => reference.Split('#', 2)[0])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var names = new Dictionary<string, string>(StringComparer.Ordinal);

        // A component that only points at a whole file gives the file its name.
        foreach (var (name, schema) in schemas)
        {
            if (schema is JsonObject { Count: 1 } alias
                && alias[Reference] is JsonValue target
                && !target.GetValue<string>().Contains('#', StringComparison.Ordinal))
            {
                names.TryAdd(target.GetValue<string>(), name);
            }
        }

        foreach (var file in referenced)
        {
            if (!files.ContainsKey(file))
            {
                throw new InvalidOperationException($"The description references '{file}', which is not among the schema files.");
            }

            if (names.ContainsKey(file))
            {
                continue;
            }

            var name = NameOf(file);
            if (schemas.ContainsKey(name) || names.ContainsValue(name))
            {
                throw new InvalidOperationException($"The name '{name}' for '{file}' is already a schema of the description.");
            }

            names[file] = name;
        }

        return names;
    }

    private static string ToLocal(string reference, Dictionary<string, string> names)
    {
        if (reference.StartsWith('#'))
        {
            return reference;
        }

        var parts = reference.Split('#', 2);
        var pointer = parts.Length == 2 ? parts[1] : string.Empty;
        return string.Concat(ComponentPrefix, names[parts[0]], pointer);
    }

    private static string NameOf(string file)
    {
        var stem = Path.GetFileName(file);
        if (stem.EndsWith(SchemaFileSuffix, StringComparison.OrdinalIgnoreCase))
        {
            stem = stem[..^SchemaFileSuffix.Length];
        }

        var words = stem.Split(['-', '_', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => string.Concat(char.ToUpper(word[0], CultureInfo.InvariantCulture).ToString(), word.AsSpan(1)));
        return string.Concat(string.Concat(words), "Schema");
    }

    private static void Rewrite(JsonNode? node, Func<string, string> rewrite)
    {
        switch (node)
        {
            case JsonObject json:
                foreach (var (key, value) in json.ToArray())
                {
                    if (key == Reference && value is JsonValue reference)
                    {
                        json[key] = rewrite(reference.GetValue<string>());
                    }
                    else
                    {
                        Rewrite(value, rewrite);
                    }
                }

                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    Rewrite(item, rewrite);
                }

                break;
        }
    }
}
