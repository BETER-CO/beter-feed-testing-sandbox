using System.Text.Json.Nodes;
using Beter.Feed.TestingSandbox.Server.Api;
using Beter.Feed.TestingSandbox.Server.Description;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Description;

public class OpenApiDocumentBundlerTests
{
    private const string ComponentPrefix = "#/components/schemas/";

    /// <summary>The schema of the served document each schema file becomes.</summary>
    private static readonly Dictionary<string, string> ComponentOfFile = new()
    {
        ["../schemas/run-output/v1/run.schema.json"] = "RunManifest",
        ["../schemas/run-output/v1/message.schema.json"] = "MessageLine",
        ["../schemas/run-output/v1/event.schema.json"] = "EventLine",
        ["../schemas/case/case.schema.json"] = "CaseSchema",
    };

    private readonly JsonObject _committed = ContractResources.LoadDocument();
    private readonly IReadOnlyDictionary<string, JsonObject> _files = ContractResources.LoadSchemas();
    private readonly JsonObject _bundle;

    public OpenApiDocumentBundlerTests()
    {
        _bundle = OpenApiDocumentBundler.Bundle(_committed, _files);
    }

    [Fact]
    public void Host_EmbedsEverySchemaFileTheDocumentReferences()
    {
        Assert.Equal(ComponentOfFile.Keys.Order(), _files.Keys.Order());
    }

    [Fact]
    public void Bundle_HasNoReferenceOutsideItself()
    {
        var references = OpenApiDocumentBundler.ReferencesOf(_bundle).ToArray();

        Assert.NotEmpty(references);
        Assert.All(references, reference => Assert.StartsWith("#/", reference, StringComparison.Ordinal));
    }

    [Fact]
    public void Bundle_ResolvesEveryReference()
    {
        var unresolved = OpenApiDocumentBundler.ReferencesOf(_bundle)
            .Distinct()
            .Where(reference => Resolve(_bundle, reference) is null)
            .ToArray();

        Assert.Empty(unresolved);
    }

    [Fact]
    public void Bundle_EmbedsEverySchemaFileUnchanged()
    {
        foreach (var (file, component) in ComponentOfFile)
        {
            var expected = _files[file].DeepClone().AsObject();
            expected.Remove("$schema");
            Rewrite(expected, reference => string.Concat(ComponentPrefix, component, reference.AsSpan(1)));

            Assert.True(
                JsonNode.DeepEquals(expected, _bundle["components"]["schemas"][component]),
                $"{component} differs from {file}.");
        }
    }

    [Fact]
    public void Bundle_KeepsTheCommittedDocumentApartFromTheReferences()
    {
        var expected = _committed.DeepClone().AsObject();
        Rewrite(expected, ToLocal);
        var expectedSchemas = expected["components"]["schemas"].AsObject();
        var actual = _bundle.DeepClone().AsObject();
        var actualSchemas = actual["components"]["schemas"].AsObject();
        foreach (var component in ComponentOfFile.Values)
        {
            expectedSchemas.Remove(component);
            actualSchemas.Remove(component);
        }

        Assert.True(JsonNode.DeepEquals(expected, actual));
    }

    [Fact]
    public void Bundle_LeavesTheCommittedDocumentUntouched()
    {
        Assert.True(JsonNode.DeepEquals(ContractResources.LoadDocument(), _committed));
    }

    [Fact]
    public void Bundle_FailsOnAReferenceToAnUnknownFile()
    {
        var document = _committed.DeepClone().AsObject();
        document["components"]["schemas"]["Health"] = new JsonObject { ["$ref"] = "../schemas/missing.schema.json" };

        var failure = Assert.Throws<InvalidOperationException>(() => OpenApiDocumentBundler.Bundle(document, _files));

        Assert.Contains("missing.schema.json", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Bundle_FailsWhenAFileNameIsAlreadyASchema()
    {
        var document = _committed.DeepClone().AsObject();
        document["components"]["schemas"]["CaseSchema"] = new JsonObject { ["type"] = "object" };

        Assert.Throws<InvalidOperationException>(() => OpenApiDocumentBundler.Bundle(document, _files));
    }

    [Fact]
    public void ServedDocument_CarriesTheVersionOfTheBuild()
    {
        var version = new SandboxVersion("9.8.7-rc.1");

        var provider = new OpenApiDocumentProvider(version);

        Assert.Equal("9.8.7-rc.1", provider.Document["info"]["version"].GetValue<string>());
        Assert.Equal(_committed["info"]["version"].GetValue<string>(), provider.CommittedVersion);
        Assert.Equal(provider.Document.ToJsonString(), JsonNode.Parse(provider.Json).ToJsonString());
    }

    /// <summary>
    /// A tag whose <c>major.minor.patch</c> differs from the committed <c>info.version</c>
    /// fails here, because a tag build passes the tag as the build version.
    /// </summary>
    [Fact]
    public void BuildVersion_HasTheCommittedDocumentVersionAsItsCore()
    {
        var build = SandboxVersion.OfThisBuild();

        Assert.Equal(_committed["info"]["version"].GetValue<string>(), build.Core);
    }

    [Theory]
    [InlineData("3.0.0", "3.0.0")]
    [InlineData("3.0.0-rc.1", "3.0.0")]
    [InlineData("3.0.0-dev", "3.0.0")]
    public void Core_DropsThePreReleaseSuffix(string version, string core)
    {
        Assert.Equal(core, new SandboxVersion(version).Core);
    }

    private static string ToLocal(string reference)
    {
        if (reference.StartsWith('#'))
        {
            return reference;
        }

        var parts = reference.Split('#', 2);
        return string.Concat(ComponentPrefix, ComponentOfFile[parts[0]], parts.Length == 2 ? parts[1] : string.Empty);
    }

    private static JsonNode Resolve(JsonNode document, string reference)
    {
        var node = document;
        foreach (var segment in reference[2..].Split('/'))
        {
            var key = segment.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
            node = node switch
            {
                JsonObject json => json[key],
                JsonArray array when int.TryParse(key, out var index) && index < array.Count => array[index],
                _ => null,
            };
            if (node is null)
            {
                return null;
            }
        }

        return node;
    }

    private static void Rewrite(JsonNode node, Func<string, string> rewrite)
    {
        switch (node)
        {
            case JsonObject json:
                foreach (var (key, value) in json.ToArray())
                {
                    if (key == "$ref" && value is JsonValue reference)
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
