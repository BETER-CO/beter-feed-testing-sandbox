using Beter.TestingTools.Generator.Application.Contracts.FeedConnections;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTools.Generator.Host.Common.Constants;

namespace Beter.TestingTools.Generator.Host.Endpoints.Connections;

public class DropConnection : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete($"{ApiConstant.ApiPrefix}/connections/{{id}}/", DropСonnectionHandler)
            .WithName("DropСonnection")
            .WithTags(ApiConstant.ConnectionTag);
    }

    private static async Task<IResult> DropСonnectionHandler(HttpContext context, string id, IFeedConnectionService feedConnectionService, CancellationToken cancellationToken = default)
    {
        await feedConnectionService.DropConnectionAsync(id, cancellationToken);

        return Results.NoContent();
    }
}
