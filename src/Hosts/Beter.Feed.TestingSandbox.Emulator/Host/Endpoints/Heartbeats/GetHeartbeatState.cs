using Beter.Feed.TestingSandbox.Emulator.Extensions;
using Beter.Feed.TestingSandbox.Emulator.Services.Heartbeats;

namespace Beter.Feed.TestingSandbox.Emulator.Host.Endpoints.Heartbeats
{
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
}
