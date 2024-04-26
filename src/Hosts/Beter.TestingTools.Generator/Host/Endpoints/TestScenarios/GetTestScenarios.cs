using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTools.Generator.Host.Common.Constants;

namespace Beter.TestingTools.Generator.Host.Endpoints.TestScenarios;

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