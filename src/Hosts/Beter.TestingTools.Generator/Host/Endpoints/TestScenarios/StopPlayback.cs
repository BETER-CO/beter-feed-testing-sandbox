using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Contracts.Requests;
using Beter.TestingTools.Generator.Contracts.Responses;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTools.Generator.Host.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Beter.TestingTools.Generator.Host.Endpoints.TestScenarios;

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
