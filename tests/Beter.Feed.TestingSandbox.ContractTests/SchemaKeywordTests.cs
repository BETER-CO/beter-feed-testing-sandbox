using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.ContractTests;

/// <summary>
/// A validator ignores a keyword it does not know, so a misspelt keyword
/// constrains nothing and no sample fails. These tests name every keyword.
/// </summary>
public class SchemaKeywordTests
{
    private static readonly HashSet<string> SchemaMaps = ["properties", "patternProperties", "$defs", "dependentSchemas"];

    private static readonly HashSet<string> SchemaLists = ["allOf", "anyOf", "oneOf", "prefixItems"];

    private static readonly HashSet<string> SingleSchemas =
    [
        "items", "contains", "additionalProperties", "unevaluatedProperties", "unevaluatedItems", "propertyNames",
        "not", "if", "then", "else", "contentSchema",
    ];

    private static readonly HashSet<string> OtherKeywords =
    [
        "$schema", "$id", "$ref", "$anchor", "$dynamicRef", "$dynamicAnchor", "$comment",
        "type", "enum", "const", "multipleOf", "maximum", "exclusiveMaximum", "minimum", "exclusiveMinimum",
        "maxLength", "minLength", "pattern", "maxItems", "minItems", "uniqueItems", "maxContains", "minContains",
        "maxProperties", "minProperties", "required", "dependentRequired",
        "title", "description", "default", "deprecated", "readOnly", "writeOnly", "examples", "format",
        "contentEncoding", "contentMediaType",
    ];

    [Theory]
    [InlineData(ContractFiles.CaseSchema)]
    [InlineData(ContractFiles.RunSchema)]
    [InlineData(ContractFiles.MessageSchema)]
    [InlineData(ContractFiles.EventSchema)]
    public void Schema_file_uses_only_JSON_Schema_keywords(string schemaPath)
    {
        Assert.Empty(UnknownKeywords(ContractFiles.Load(schemaPath), schemaPath));
    }

    [Fact]
    public void OpenApi_schemas_use_only_JSON_Schema_keywords()
    {
        var document = ContractFiles.Load(ContractFiles.OpenApiDocument);
        var schemas = document["components"]!["schemas"]!.AsObject()
            .Select(schema => (schema.Value, Location: $"components/schemas/{schema.Key}"))
            .Concat(InlineSchemas(document["paths"], "paths"))
            .Concat(InlineSchemas(document["components"]!["parameters"], "components/parameters"))
            .Concat(InlineSchemas(document["components"]!["responses"], "components/responses"))
            .ToArray();

        Assert.NotEmpty(schemas);
        Assert.Empty(schemas.SelectMany(schema => UnknownKeywords(schema.Value, schema.Location)));
    }

    /// <summary>
    /// The schemas the OpenAPI document holds inline: the value of every <c>schema</c> member.
    /// </summary>
    private static IEnumerable<(JsonNode? Value, string Location)> InlineSchemas(JsonNode? node, string location) => node switch
    {
        JsonObject json => json.SelectMany(member => member.Key == "schema"
            ? [(member.Value, $"{location}/schema")]
            : InlineSchemas(member.Value, $"{location}/{member.Key}")),
        JsonArray array => array.SelectMany((item, index) => InlineSchemas(item, $"{location}/{index}")),
        _ => [],
    };

    private static IEnumerable<string> UnknownKeywords(JsonNode? schema, string location)
    {
        if (schema is not JsonObject json)
        {
            yield break;
        }

        foreach (var (keyword, value) in json)
        {
            var here = $"{location}/{keyword}";
            IEnumerable<string> unknown =
                SchemaMaps.Contains(keyword) ? value!.AsObject().SelectMany(member => UnknownKeywords(member.Value, $"{here}/{member.Key}"))
                : SchemaLists.Contains(keyword) ? value!.AsArray().SelectMany((item, index) => UnknownKeywords(item, $"{here}/{index}"))
                : SingleSchemas.Contains(keyword) ? UnknownKeywords(value, here)
                : OtherKeywords.Contains(keyword) ? []
                : [here];
            foreach (var found in unknown)
            {
                yield return found;
            }
        }
    }
}
