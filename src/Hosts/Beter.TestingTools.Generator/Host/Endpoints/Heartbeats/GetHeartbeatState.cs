using Beter.TestingTool.Generator.Application.Contracts.Heartbeats;
using Beter.TestingTool.Generator.Application.Services.Heartbeats;
using Beter.TestingTool.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTool.Generator.Host.Common.Constants;

namespace Beter.TestingTool.Generator.Host.Endpoints.Heartbeats;

public class GetHeartbeatState : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{ApiConstant.ApiPrefix}/heartbeats", GetHeartbeatCommandHandler)
            .WithName("GetCurrentHeartbeatState")
            .Produces<HeartbeatCommand>()
            .WithTags(ApiConstant.HeartbeatTag);
    }

    private static Task<IResult> GetHeartbeatCommandHandler(IHeartbeatControlService heartbeatControlService)
    {
        var command = heartbeatControlService.GetCurrentCommand();

        return Task.FromResult(Results.Ok(command));
    }
}