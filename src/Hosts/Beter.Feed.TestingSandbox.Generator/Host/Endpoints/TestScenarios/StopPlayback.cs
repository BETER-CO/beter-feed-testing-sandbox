using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Contracts.Responses;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.TestScenarios;

public class StopPlayback : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{ApiConstant.ApiPrefix}/test-scenarios/stop", StopPlaybackHandler)
            .WithName("StopPlayback")
            .Accepts<StopPlaybackRequest>(ApiConstant.ContentType)
            .Produces<StopPlaybackItemResponse>()
            .WithTags(ApiConstant.TestScenarioTag);
    }

    private static IResult StopPlaybackHandler(StopPlaybackRequest request, ITestScenarioService testScenarioService)
    {
        var response = testScenarioService.Stop(request);

        return Results.Ok(response);
    }
}
