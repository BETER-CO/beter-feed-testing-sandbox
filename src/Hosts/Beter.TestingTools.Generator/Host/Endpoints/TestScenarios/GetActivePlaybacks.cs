using Beter.TestingTool.Generator.Application.Mappers;
using Beter.TestingTool.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Contracts.Playbacks;
using Beter.TestingTool.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTool.Generator.Host.Common.Constants;

namespace Beter.TestingTool.Generator.Host.Endpoints.TestScenarios;

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