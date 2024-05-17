using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Consumer.Domain;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Consumer.Services.Abstract
{
    public interface ITestScenarioTemplateService
    {
        JsonNode GetNext(HubKind hubKind);
        TestScenarioTemplate GetTemplate();
        TestScenarioTemplate SetTemplate(TestScenario testScenario);
        TestScenarioTemplate SetMissmatchItem(HubKind hubKind, string expected, string actual);
    }
}
