using Beter.Feed.TestingSandbox.Generator.Application.Contracts.Heartbeats;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Heartbeats;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.Heartbeats;

public class SetHeartbeatCommand : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{ApiConstant.ApiPrefix}/heartbeats", SetHeartbeatCommandHandler)
            .WithName("SetHeartbeatCommandAsync")
            .Accepts<SetHeartbeatCommandRequest>(ApiConstant.ContentType)
            .Produces<HeartbeatCommand>()
            .WithTags(ApiConstant.HeartbeatTag);
    }

    private static Task<IResult> SetHeartbeatCommandHandler(SetHeartbeatCommandRequest request, IHeartbeatControlService heartbeatControlService)
    {
        var response = heartbeatControlService.SetCommand(request.Command);

        return Task.FromResult(Results.Ok(response));
    }
}