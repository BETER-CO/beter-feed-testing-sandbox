using Beter.TestingTool.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Application.Mappers;
using Beter.TestingTool.Generator.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.TestingTool.Generator.Host.Common.Constants;

namespace Beter.TestingTool.Generator.Host.Endpoints.TestScenarios;

public sealed class LoadTestScenario : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{ApiConstant.ApiPrefix}/test-scenarios/load", LoadTestScenarioHandler)
            .WithName("LoadTestScenario")
            .Produces<TestScenarioDto>()
            .WithTags(ApiConstant.TestScenarioTag);
    }

    private async static Task<IResult> LoadTestScenarioHandler(IFormFile file, ITestScenariosRepository repository, ITestScenarioFactory factory)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("File is empty.");

        using (var stream = file.OpenReadStream())
        {
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            if (!int.TryParse(fileName, out var caseId))
                return Results.BadRequest("Invalid file name format.");

            //TODO: Add file content validation
            var testScenario = await factory.Create(caseId, stream);
            repository.Add(testScenario);

            return Results.Ok(TestScenarioMapper.MapToDto(testScenario));
        }
    }
}