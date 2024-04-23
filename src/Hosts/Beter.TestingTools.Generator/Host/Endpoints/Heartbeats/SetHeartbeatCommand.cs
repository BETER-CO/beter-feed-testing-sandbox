using Beter.TestingTool.Generator.Application.Contracts.Heartbeats;
using Beter.TestingTool.Generator.Contracts.Requests;
using Beter.TestingTool.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTool.Generator.Host.Common.Constants;

namespace Beter.TestingTool.Generator.Host.Endpoints.Heartbeats;

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