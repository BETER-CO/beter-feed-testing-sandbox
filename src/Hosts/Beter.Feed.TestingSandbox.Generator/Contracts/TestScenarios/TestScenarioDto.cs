namespace Beter.Feed.TestingSandbox.Generator.Contracts.TestScenarios;

public sealed record TestScenarioDto
{
    public int CaseId { get; init; }
    public string Version { get; init; }
    public string Description { get; init; }
}
