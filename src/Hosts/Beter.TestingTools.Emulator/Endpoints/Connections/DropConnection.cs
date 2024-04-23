using Beter.TestingTools.Emulator.Extensions;
using Beter.TestingTools.Emulator.Services;

namespace Beter.TestingTools.Emulator.Endpoints.Connections;

public class DropConnection : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete($"{ApiConstant.ApiPrefix}/connections/{{id}}/", DropСonnectionHandler)
            .WithName("DropСonnection")
            .WithTags(ApiConstant.ConnectionTag);
    }

    private static IResult DropСonnectionHandler(HttpContext context, string id, IConnectionManager connectionManager)
    {
        connectionManager.Remove(id);

        return Results.NoContent();
    }
}
