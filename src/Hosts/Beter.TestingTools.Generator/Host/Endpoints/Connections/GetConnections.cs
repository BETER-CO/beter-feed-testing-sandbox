using Beter.TestingTools.Common.Models;
using Beter.TestingTool.Generator.Application.Contracts.FeedConnections;
using Beter.TestingTool.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTool.Generator.Host.Common.Constants;

namespace Beter.TestingTool.Generator.Host.Endpoints.Connections;

public class GetConnections : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{ApiConstant.ApiPrefix}/connections", GetСonnectionsHandler)
            .WithName("GetСonnections")
            .Produces<IEnumerable<FeedConnection>>()
            .WithTags(ApiConstant.ConnectionTag);
    }

    private static async Task<IResult> GetСonnectionsHandler(HttpContext context, IFeedConnectionService feedConnectionService, CancellationToken cancellationToken = default)
    {
        var connections = await feedConnectionService.GetAsync(cancellationToken);

        return Results.Ok(connections);
    }
}
