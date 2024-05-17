using Beter.Feed.TestingSandbox.Consumer.Domain;
using Beter.Feed.TestingSandbox.Consumer.Endpoints;
using Beter.Feed.TestingSandbox.Consumer.Extensions;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Beter.Feed.TestingSandbox.Consumer.Endpoints.TestScenarios
{
    public class GetTestScenarioTemplate : IEndpointProvider
    {
        public static void DefineEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet($"{ApiConstant.ApiPrefix}/test-scenario-templates", GetTestScenarioTemplateHandler)
                .WithName("GetTestScenarioTemplateTemplate")
                .Produces<TestScenarioTemplate>()
                .WithTags(ApiConstant.TestScenarioTemplateTag);
        }

        private static IResult GetTestScenarioTemplateHandler(ITestScenarioTemplateService testScenarioTemplate)
        {
            var response = testScenarioTemplate.GetTemplate();
            if (response == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(response);
        }
    }
}
