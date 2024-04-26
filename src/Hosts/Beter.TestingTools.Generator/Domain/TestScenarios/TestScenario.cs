namespace Beter.TestingTools.Generator.Domain.TestScenarios;

public sealed record TestScenario
{
    public const string ResourcesPath = "Resources";

    public int CaseId { get; set; }
    public Version Version { get; set; }
    public string Description { get; set; }
    public IEnumerable<TestScenarioMessage> Messages { get; init; }

    public void SetCaseId(int caseId) => CaseId = caseId;
}