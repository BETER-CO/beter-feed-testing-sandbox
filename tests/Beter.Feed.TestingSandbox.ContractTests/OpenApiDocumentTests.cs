using System.Text.Json.Nodes;
using LateApexEarlySpeed.Json.Schema;

namespace Beter.Feed.TestingSandbox.ContractTests;

public class OpenApiDocumentTests
{
    private static readonly string[] Methods = ["get", "put", "post", "delete", "options", "head", "patch", "trace"];

    /// <summary>
    /// The thirteen operations of the client contract, one row each; the
    /// description endpoint is one operation served as JSON and rendered.
    /// </summary>
    private static readonly string[][] ThirteenOperations =
    [
        ["GET /api/version"],
        ["GET /api/health"],
        ["GET /api/cases"],
        ["GET /api/cases/{caseId}"],
        ["POST /api/cases"],
        ["POST /api/runs"],
        ["GET /api/runs"],
        ["GET /api/runs/{runId}"],
        ["GET /api/runs/{runId}/messages"],
        ["GET /api/runs/{runId}/events"],
        ["POST /api/runs/{runId}/stop"],
        ["DELETE /api/runs/{runId}"],
        ["GET /openapi.json", "GET /swagger"],
    ];

    /// <summary>
    /// The error responses of the client contract, per operation.
    /// </summary>
    private static readonly Dictionary<string, string[]> ErrorResponses = new()
    {
        ["GET /api/health"] = ["503"],
        ["GET /api/cases/{caseId}"] = ["404"],
        ["POST /api/cases"] = ["422"],
        ["POST /api/runs"] = ["400", "404", "410"],
        ["GET /api/runs/{runId}"] = ["404"],
        ["GET /api/runs/{runId}/messages"] = ["404"],
        ["GET /api/runs/{runId}/events"] = ["404"],
        ["POST /api/runs/{runId}/stop"] = ["404", "409"],
        ["DELETE /api/runs/{runId}"] = ["404", "409"],
    };

    private static readonly JsonNode Document = ContractFiles.Load(ContractFiles.OpenApiDocument);

    [Fact]
    public void OpenApi_document_validates_as_OpenAPI_3_1()
    {
        var openApiSchema = new JsonValidator(ContractFiles.ReadResource("oas-3.1-schema-2022-10-07.json"));

        Assert.Null(openApiSchema.Failure(Document));
        Assert.StartsWith("3.1.", Document["openapi"]!.GetValue<string>());
    }

    [Fact]
    public void OpenApi_document_names_the_thirteen_operations()
    {
        var expected = ThirteenOperations.SelectMany(row => row).Order().ToArray();

        var actual = Operations().Select(operation => operation.Name).Order().ToArray();

        Assert.Equal(expected, actual);
        Assert.Equal(12, actual.Count(name => name.Contains(" /api/")));
        Assert.Equal(new[] { "GET /openapi.json", "GET /swagger" }, actual.Where(name => !name.Contains(" /api/")));
    }

    [Fact]
    public void OpenApi_document_names_the_error_responses_of_the_client_contract()
    {
        foreach (var (name, operation) in Operations())
        {
            var expected = ErrorResponses.GetValueOrDefault(name, []);

            var errors = operation["responses"]!.AsObject()
                .Where(response => response.Key[0] is '4' or '5')
                .ToDictionary(response => response.Key, response => response.Value!);

            Assert.True(
                expected.SequenceEqual(errors.Keys.Order()),
                $"{name}: expected [{string.Join(", ", expected)}], found [{string.Join(", ", errors.Keys.Order())}]");
            foreach (var (status, response) in errors)
            {
                var body = Resolve(response)["content"]!["application/json"]!["schema"]!;
                var required = RequiredKeys(body);
                Assert.True(required.Contains("error") && required.Contains("detail"), $"{name} {status}: body without error and detail");
            }
        }
    }

    [Fact]
    public void A_retired_case_is_refused_with_its_replacement()
    {
        var gone = Operations().Single(operation => operation.Name == "POST /api/runs").Operation["responses"]!["410"]!;

        var required = RequiredKeys(gone["content"]!["application/json"]!["schema"]!);

        Assert.Contains("replacedBy", required);
    }

    [Fact]
    public void OpenApi_info_version_is_the_sandbox_version()
    {
        Assert.Equal("3.0.0", Document["info"]!["version"]!.GetValue<string>());
    }

    [Fact]
    public void Every_reference_of_the_OpenApi_document_resolves()
    {
        var documentPath = ContractFiles.PathOf(ContractFiles.OpenApiDocument);
        var pending = new Queue<string>([documentPath]);
        var visited = new HashSet<string>();
        var references = 0;

        while (pending.TryDequeue(out var path))
        {
            if (!visited.Add(path))
            {
                continue;
            }

            var file = ContractFiles.LoadFile(path);
            foreach (var reference in References(file))
            {
                var (targetPath, target) = Follow(path, file, reference);
                Assert.True(target is not null, $"{ContractFiles.RelativePathOf(path)}: {reference} does not resolve");
                pending.Enqueue(targetPath);
                references++;
            }
        }

        Assert.NotEqual(0, references);
    }

    [Fact]
    public void Case_and_run_output_schemas_are_referenced_not_copied()
    {
        var documentPath = ContractFiles.PathOf(ContractFiles.OpenApiDocument);
        var referencedFiles = References(Document)
            .Select(reference => ContractFiles.RelativePathOf(Follow(documentPath, Document, reference).Path))
            .Where(path => path != ContractFiles.OpenApiDocument)
            .Distinct()
            .Order()
            .ToArray();
        var schemas = Document["components"]!["schemas"]!;
        var caseFields = new[] { "caseId", "version", "class", "sport", "tags", "description", "terminal", "duration" };

        Assert.Equal(
            new[] { ContractFiles.CaseSchema, ContractFiles.EventSchema, ContractFiles.MessageSchema, ContractFiles.RunSchema },
            referencedFiles);
        Assert.All(caseFields, field => Assert.StartsWith(
            "{\"$ref\":\"../schemas/case/case.schema.json#/$defs/",
            schemas["CatalogueRow"]!["properties"]![field]!.ToJsonString()));
        Assert.StartsWith(
            "{\"$ref\":\"../schemas/case/case.schema.json#/$defs/caseBody\"}",
            schemas["RuntimeCase"]!["allOf"]![0]!.ToJsonString());
        Assert.StartsWith(
            "{\"$ref\":\"../schemas/case/case.schema.json#/$defs/caseBody\"}",
            schemas["CaseDetail"]!["allOf"]![0]!.ToJsonString());
        Assert.Equal("""{"$ref":"../schemas/run-output/v1/run.schema.json"}""", schemas["RunManifest"]!.ToJsonString());
        Assert.Equal("""{"$ref":"../schemas/run-output/v1/message.schema.json"}""", schemas["MessageLine"]!.ToJsonString());
        Assert.Equal("""{"$ref":"../schemas/run-output/v1/event.schema.json"}""", schemas["EventLine"]!.ToJsonString());
    }

    private static IEnumerable<(string Name, JsonNode Operation)> Operations() =>
        from path in Document["paths"]!.AsObject()
        from member in path.Value!.AsObject()
        where Methods.Contains(member.Key)
        select ($"{member.Key.ToUpperInvariant()} {path.Key}", member.Value!);

    private static IEnumerable<string> References(JsonNode? node) => node switch
    {
        JsonObject json => json.SelectMany(member =>
            member.Key == "$ref" && member.Value is JsonValue
                ? [member.Value.GetValue<string>()]
                : References(member.Value)),
        JsonArray array => array.SelectMany(References),
        _ => [],
    };

    /// <summary>
    /// Follows a reference from a file: the target file and the node the JSON Pointer names.
    /// </summary>
    private static (string Path, JsonNode? Target) Follow(string fromPath, JsonNode fromFile, string reference)
    {
        var parts = reference.Split('#', 2);
        var path = parts[0].Length == 0
            ? fromPath
            : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fromPath)!, parts[0]));
        if (!File.Exists(path))
        {
            return (path, null);
        }

        var target = parts[0].Length == 0 ? fromFile : ContractFiles.LoadFile(path);
        var pointer = parts.Length == 2 ? parts[1] : string.Empty;
        foreach (var token in pointer.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            var key = token.Replace("~1", "/").Replace("~0", "~");
            target = target switch
            {
                JsonObject json => json[key],
                JsonArray array when int.TryParse(key, out var index) && index < array.Count => array[index],
                _ => null,
            };
            if (target is null)
            {
                break;
            }
        }

        return (path, target);
    }

    private static JsonNode Resolve(JsonNode node)
    {
        while (node["$ref"] is JsonValue reference)
        {
            node = Follow(ContractFiles.PathOf(ContractFiles.OpenApiDocument), Document, reference.GetValue<string>()).Target!;
        }

        return node;
    }

    /// <summary>
    /// The keys a schema of the document requires, through its references and its allOf.
    /// </summary>
    private static HashSet<string> RequiredKeys(JsonNode schema)
    {
        schema = Resolve(schema);
        var required = new HashSet<string>();
        if (schema["required"] is JsonArray keys)
        {
            required.UnionWith(keys.Select(key => key!.GetValue<string>()));
        }

        if (schema["allOf"] is JsonArray parts)
        {
            foreach (var part in parts)
            {
                required.UnionWith(RequiredKeys(part!));
            }
        }

        return required;
    }
}
