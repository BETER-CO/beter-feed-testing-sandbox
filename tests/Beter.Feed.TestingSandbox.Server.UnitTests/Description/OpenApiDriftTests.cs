using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Beter.Feed.TestingSandbox.Server.Api;
using Beter.Feed.TestingSandbox.Server.Description;
using Beter.Feed.TestingSandbox.Server.UnitTests.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Beter.Feed.TestingSandbox.Server.UnitTests.Description;

/// <summary>
/// The drift check: the operations the host maps, generated from the code, against the
/// operations <c>openapi/sandbox.yaml</c> describes. A difference in either direction fails.
/// </summary>
public partial class OpenApiDriftTests : IClassFixture<SandboxHost>
{
    private static readonly string[] HttpMethods = ["get", "put", "post", "delete", "patch", "head", "options"];

    private readonly SandboxHost _host;

    public OpenApiDriftTests(SandboxHost host)
    {
        _host = host;
    }

    [Fact]
    public void MappedOperations_AreTheOperationsOfTheCommittedDocument()
    {
        var fromCode = OperationsOfTheCode().ToArray();

        // The rendered description is served by the Swagger UI middleware, not by a mapped
        // endpoint; SandboxApiTests.GetSwagger_RendersTheServedDescription covers it.
        var fromDocument = OperationsOfTheDocument()
            .Where(operation => !operation.StartsWith($"GET {SandboxApi.SwaggerUiPath} ", StringComparison.Ordinal))
            .ToArray();

        var onlyInDocument = fromDocument.Except(fromCode).Order(StringComparer.Ordinal).ToArray();
        var onlyInCode = fromCode.Except(fromDocument).Order(StringComparer.Ordinal).ToArray();

        Assert.True(
            onlyInDocument.Length == 0 && onlyInCode.Length == 0,
            $"openapi/sandbox.yaml and the mapped endpoints differ.{Environment.NewLine}"
            + $"Only in the document:{Environment.NewLine}{string.Join(Environment.NewLine, onlyInDocument)}{Environment.NewLine}"
            + $"Only in the code:{Environment.NewLine}{string.Join(Environment.NewLine, onlyInCode)}");
        Assert.Equal(fromDocument.Length, fromCode.Length);
    }

    [Fact]
    public void RenderedDescription_IsTheOnlyOperationOutsideTheMappedEndpoints()
    {
        var rendered = OperationsOfTheDocument()
            .Where(operation => operation.StartsWith($"GET {SandboxApi.SwaggerUiPath} ", StringComparison.Ordinal));

        Assert.Single(rendered);
    }

    [GeneratedRegex(@"\{(\w+)[:?][^}]*\}")]
    private static partial Regex RouteConstraint();

    private static string Describe(
        string method,
        string path,
        string operationId,
        IEnumerable<string> parameters,
        IEnumerable<string> bodies,
        IEnumerable<string> responses) => string.Join(
            ' ',
            method.ToUpperInvariant(),
            path,
            operationId,
            $"parameters[{string.Join(',', parameters.Order(StringComparer.Ordinal))}]",
            $"body[{string.Join('+', bodies.Order(StringComparer.Ordinal))}]",
            $"responses[{string.Join(',', responses.Order(StringComparer.Ordinal))}]");

    private static string DescribeParameter(string location, string name, bool required, string type) =>
        $"{location}:{name}:{(required ? "required" : "optional")}:{type}";

    private static JsonNode Resolve(JsonObject document, JsonNode node)
    {
        while (node is JsonObject json && json["$ref"] is JsonValue reference)
        {
            node = document;
            foreach (var segment in reference.GetValue<string>().TrimStart('#', '/').Split('/'))
            {
                node = node[segment];
            }
        }

        return node;
    }

    private static string TypeOf(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type == typeof(string))
        {
            return "string";
        }

        if (type.IsArray)
        {
            return "array";
        }

        if (type == typeof(int) || type == typeof(long))
        {
            return "integer";
        }

        return type == typeof(bool) ? "boolean" : type.Name;
    }

    /// <summary>
    /// The operations of the committed document, read with its schema references resolved.
    /// </summary>
    private static IEnumerable<string> OperationsOfTheDocument()
    {
        var document = OpenApiDocumentBundler.Bundle(ContractResources.LoadDocument(), ContractResources.LoadSchemas());
        foreach (var (path, item) in document["paths"].AsObject())
        {
            var shared = item["parameters"]?.AsArray() ?? [];
            foreach (var (method, operation) in item.AsObject().Where(entry => HttpMethods.Contains(entry.Key)))
            {
                var parameters = shared.Concat(operation["parameters"]?.AsArray() ?? [])
                    .Select(parameter => Resolve(document, parameter))
                    .Select(parameter => DescribeParameter(
                        parameter["in"].GetValue<string>(),
                        parameter["name"].GetValue<string>(),
                        parameter["required"]?.GetValue<bool>() ?? false,
                        Resolve(document, parameter["schema"])["type"].GetValue<string>()));
                var bodies = operation["requestBody"]?["content"]?.AsObject().Select(type => type.Key) ?? [];
                var responses = operation["responses"].AsObject().Select(response =>
                {
                    var content = Resolve(document, response.Value)["content"]?.AsObject();
                    return $"{response.Key}:{string.Join('+', content?.Select(type => type.Key) ?? [])}";
                });

                yield return Describe(
                    method,
                    path,
                    operation["operationId"].GetValue<string>(),
                    parameters,
                    bodies,
                    responses);
            }
        }
    }

    private IEnumerable<string> OperationsOfTheCode()
    {
        var explorer = _host.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
        foreach (var api in explorer.ApiDescriptionGroups.Items.SelectMany(group => group.Items))
        {
            var operationId = api.ActionDescriptor.EndpointMetadata.OfType<IEndpointNameMetadata>().Single().EndpointName;
            var parameters = api.ParameterDescriptions
                .Where(parameter => parameter.Source.Id != "Body")
                .Select(parameter => DescribeParameter(
                    parameter.Source.Id.ToLowerInvariant(),
                    parameter.Name,
                    parameter.IsRequired,
                    TypeOf(parameter.Type)));
            var responses = api.SupportedResponseTypes.Select(response =>
                $"{response.StatusCode}:{string.Join('+', response.ApiResponseFormats.Select(format => format.MediaType))}");

            yield return Describe(
                api.HttpMethod,
                $"/{RouteConstraint().Replace(api.RelativePath, "{$1}")}",
                operationId,
                parameters,
                api.SupportedRequestFormats.Select(format => format.MediaType),
                responses);
        }
    }
}
