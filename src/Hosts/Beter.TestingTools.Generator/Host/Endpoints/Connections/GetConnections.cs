using Beter.TestingTools.Common.Models;
using Beter.TestingTools.Generator.Host.Common.Constants;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTools.Generator.Application.Contracts.FeedConnections;

namespace Beter.TestingTools.Generator.Host.Endpoints.Connections;

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
