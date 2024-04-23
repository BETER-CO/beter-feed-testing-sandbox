using Beter.TestingTools.Common.Models;
using Beter.TestingTools.Emulator.Extensions;
using Beter.TestingTools.Emulator.Services;

namespace Beter.TestingTools.Emulator.Endpoints.Connections;

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
