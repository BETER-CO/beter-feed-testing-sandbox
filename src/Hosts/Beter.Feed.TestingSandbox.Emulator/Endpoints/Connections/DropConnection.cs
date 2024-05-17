using Beter.Feed.TestingSandbox.Emulator.Extensions;
using Beter.Feed.TestingSandbox.Emulator.Services;

namespace Beter.Feed.TestingSandbox.Emulator.Endpoints.Connections;

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
