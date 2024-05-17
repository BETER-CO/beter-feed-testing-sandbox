using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.TestScenarios;

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