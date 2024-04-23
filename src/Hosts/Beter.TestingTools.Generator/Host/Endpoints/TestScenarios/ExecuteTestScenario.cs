using Beter.TestingTool.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Contracts.Requests;
using Beter.TestingTool.Generator.Domain.Playbacks;
using Beter.TestingTool.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTool.Generator.Host.Common.Constants;

namespace Beter.TestingTool.Generator.Host.Endpoints.TestScenarios;

public class ExecuteTestScenario : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{ApiConstant.ApiPrefix}/test-scenarios/run", RunTestScenarioHandler)
            .WithName("StartTestScenario")
            .Accepts<StartPlaybackRequest>(ApiConstant.ContentType)
            .Produces<Playback>()
            .WithTags(ApiConstant.TestScenarioTag);
    }

    private static IResult RunTestScenarioHandler(StartPlaybackRequest request, ITestScenarioService testScenarioService, CancellationToken ct)
    {
        var response = testScenarioService.Start(request);

        return Results.Ok(response);
    }
}