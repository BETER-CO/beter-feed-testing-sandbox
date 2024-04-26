using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Contracts.Requests;
using Beter.TestingTools.Generator.Domain.Playbacks;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTools.Generator.Host.Common.Constants;

namespace Beter.TestingTools.Generator.Host.Endpoints.TestScenarios;

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