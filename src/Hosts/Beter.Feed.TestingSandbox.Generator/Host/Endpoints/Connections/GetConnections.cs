using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.FeedConnections;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.Connections;

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
