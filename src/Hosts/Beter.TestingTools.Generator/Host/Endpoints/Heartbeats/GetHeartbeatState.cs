using Beter.TestingTools.Generator.Application.Contracts.Heartbeats;
using Beter.TestingTools.Generator.Application.Services.Heartbeats;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTools.Generator.Host.Common.Constants;

namespace Beter.TestingTools.Generator.Host.Endpoints.Heartbeats;

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