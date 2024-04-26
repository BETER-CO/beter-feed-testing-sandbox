using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Contracts.Playbacks;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTools.Generator.Host.Common.Constants;

namespace Beter.TestingTools.Generator.Host.Endpoints.TestScenarios;

public class GetActivePlaybacks : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{ApiConstant.ApiPrefix}/test-scenarios/active", GetActivePlaybacksHandler)
            .WithName("ShowAllActivePlaybacks")
            .Produces<IEnumerable<PlaybackDto>>()
            .WithTags(ApiConstant.TestScenarioTag);
    }

    private static IResult GetActivePlaybacksHandler(ITestScenarioService testScenarioService)
    {
        var response = testScenarioService.GetActivePlaybacks();

        return Results.Ok(response);
    }
}