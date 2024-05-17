using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.TestScenarios;

public class GetTestScenarios : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{ApiConstant.ApiPrefix}/test-scenarios", GetTestScenariosHandler)
            .WithName("GetAllTestScenarios")
            .Produces<TestScenarioDto>()
            .WithTags(ApiConstant.TestScenarioTag);
    }

    private static IResult GetTestScenariosHandler(ITestScenarioService testScenarioService)
    {
        var response = testScenarioService.GetAll();

        return Results.Ok(response);
    }
}