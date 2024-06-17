using Beter.Feed.TestingSandbox.Emulator.Extensions;
using Beter.Feed.TestingSandbox.Emulator.Services.Heartbeats;

namespace Beter.Feed.TestingSandbox.Emulator.Host.Endpoints.Heartbeats
{
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

    public record SetHeartbeatCommandRequest
    {
        public int Command { get; init; }
    }
}
