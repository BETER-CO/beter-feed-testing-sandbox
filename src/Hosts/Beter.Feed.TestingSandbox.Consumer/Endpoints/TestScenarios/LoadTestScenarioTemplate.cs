using Beter.Feed.TestingSandbox.Consumer.Endpoints;
using Beter.Feed.TestingSandbox.Consumer.Extensions;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Beter.Feed.TestingSandbox.Consumer.Endpoints.TestScenarios
{
    public sealed class LoadTestScenarioTemplate : IEndpointProvider
    {
        public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost($"{ApiConstant.ApiPrefix}/test-scenario-templates/load", LoadTestScenarioTemplateHandler)
                .WithName("LoadTestScenarioTemplate")
                .WithTags(ApiConstant.TestScenarioTemplateTag);
        }

        private async static Task<IResult> LoadTestScenarioTemplateHandler(IFormFile file, ITestScenarioTemplateService templateService, ITestScenarioFactory factory)
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("File is empty.");

            using (var stream = file.OpenReadStream())
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                if (!int.TryParse(fileName, out var caseId))
                    return Results.BadRequest("Invalid file name format.");

                var testScenario = await factory.Create(caseId, stream);
                templateService.SetTemplate(testScenario);

                return Results.Ok();
            }
        }
    }
}