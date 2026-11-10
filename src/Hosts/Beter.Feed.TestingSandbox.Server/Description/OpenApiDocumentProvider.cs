#nullable enable

using System.Text.Json;
using System.Text.Json.Nodes;
using Beter.Feed.TestingSandbox.Server.Api;

namespace Beter.Feed.TestingSandbox.Server.Description;

/// <summary>
/// The description the container serves at <c>/openapi.json</c>: the committed document
/// with its schemas inlined and <c>info.version</c> set to the version of this build.
/// </summary>
public sealed class OpenApiDocumentProvider
{
    /// <summary>The name the rendered description shows.</summary>
    public const string Title = "BETER Feed Testing Sandbox";

    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    /// <summary>Initializes a new instance of the <see cref="OpenApiDocumentProvider"/> class.</summary>
    /// <param name="version">The version of this build.</param>
    public OpenApiDocumentProvider(SandboxVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        var committed = ContractResources.LoadDocument();
        CommittedVersion = committed["info"]!["version"]!.GetValue<string>();

        Document = OpenApiDocumentBundler.Bundle(committed, ContractResources.LoadSchemas());
        Document["info"]!["version"] = version.Value;
        Json = JsonSerializer.SerializeToUtf8Bytes(Document, Indented);
    }

    /// <summary>Gets <c>info.version</c> of the committed document.</summary>
    public string CommittedVersion { get; }

    /// <summary>Gets the served document.</summary>
    public JsonObject Document { get; }

    /// <summary>Gets the served document as UTF-8 JSON.</summary>
    public byte[] Json { get; }
}
