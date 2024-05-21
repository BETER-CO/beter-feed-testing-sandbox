using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;
using FluentValidation;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.TestScenarios;

public class ExecuteTestScenario : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{ApiConstant.ApiPrefix}/test-scenarios/run", RunTestScenarioHandler)
            .WithName("StartTestScenario")
            .Accepts<StartPlaybackRequest>(ApiConstant.ContentType)
            .Produces<PlaybackDto>()
            .WithTags(ApiConstant.TestScenarioTag);
    }

    private static IResult RunTestScenarioHandler(StartPlaybackRequest request, IValidator<StartPlaybackRequest> validator, ITestScenarioService testScenarioService, CancellationToken ct)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage);

            return Results.BadRequest(new { Errors = errorMessages });
        }

        var response = testScenarioService.Start(request);

        return Results.Ok(response);
    }
}