using Beter.Feed.TestingSandbox.Common.Models;

namespace Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

public sealed record TestScenario
{
    public const string ResourcesPath = "Resources";

    public int CaseId { get; set; }
    public Version Version { get; set; }
    public string Description { get; set; }
    public AdditionalInfo AdditionalInfo { get; set; } = AdditionalInfo.NoInfo();
    public IEnumerable<TestScenarioMessage> Messages { get; init; }

    public void SetCaseId(int caseId) => CaseId = caseId;
}