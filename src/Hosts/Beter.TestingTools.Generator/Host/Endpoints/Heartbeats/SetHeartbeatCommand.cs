using Beter.TestingTools.Generator.Application.Contracts.Heartbeats;
using Beter.TestingTools.Generator.Contracts.Requests;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTools.Generator.Host.Common.Constants;

namespace Beter.TestingTools.Generator.Host.Endpoints.Heartbeats;

public class SetHeartbeatCommand : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{ApiConstant.ApiPrefix}/heartbeats", SetHeartbeatCommandHandler)
            .WithName("SetHeartbeatCommandAsync")
            .Accepts<SetHeartbeatCommandRequest>(ApiConstant.ContentType)
            .WithTags(ApiConstant.HeartbeatTag);
    }

    private static Task<IResult> SetHeartbeatCommandHandler(SetHeartbeatCommandRequest request, IHeartbeatControlService heartbeatControlService)
    {
        var response = heartbeatControlService.SetCommand(request.Command);

        return Task.FromResult(Results.Ok(response));
    }
}