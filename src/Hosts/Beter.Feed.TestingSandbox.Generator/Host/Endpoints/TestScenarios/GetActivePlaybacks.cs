﻿using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.TestScenarios;

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