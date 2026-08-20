using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Application.Mappers;
using Beter.Feed.TestingSandbox.Generator.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using Beter.Feed.TestingSandbox.Generator.Host.Common.Constants;

namespace Beter.Feed.TestingSandbox.Generator.Host.Endpoints.TestScenarios;

public sealed class LoadTestScenario : IEndpointProvider
{
    public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{ApiConstant.ApiPrefix}/test-scenarios/load", LoadTestScenarioHandler)
            .WithName("LoadTestScenario")
            .Produces<TestScenarioDto>()
            // Binding IFormFile adds anti-forgery metadata, which would reject every upload unless
            // the anti-forgery middleware is registered. This is a machine-to-machine REST API with
            // no cookie authentication, so there is no request forgery to protect against and no way
            // for a client to supply a token.
            .DisableAntiforgery()
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
            repository.AddOrUpdate(testScenario);

            return Results.Ok(TestScenarioMapper.MapToDto(testScenario));
        }
    }
}