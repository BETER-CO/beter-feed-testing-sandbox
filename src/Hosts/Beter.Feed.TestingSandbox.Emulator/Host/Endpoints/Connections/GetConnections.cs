using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Emulator.Extensions;
using Beter.Feed.TestingSandbox.Emulator.Services.Connections;

namespace Beter.Feed.TestingSandbox.Emulator.Host.Endpoints.Connections;

public class GetConnections : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{ApiConstant.ApiPrefix}/connections", GetСonnectionsHandler)
            .WithName("GetСonnections")
            .Produces<IEnumerable<FeedConnection>>()
            .WithTags(ApiConstant.ConnectionTag);
    }

    private static IResult GetСonnectionsHandler(HttpContext context, IConnectionManager connectionManager)
    {
        return Results.Ok(connectionManager.GetAll());
    }
}
