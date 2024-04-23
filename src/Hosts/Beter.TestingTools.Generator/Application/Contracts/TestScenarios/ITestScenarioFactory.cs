using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioFactory
{
    Task<TestScenario> Create(int caseId, Stream stream);
    IEnumerable<TestScenario> Create(string testScenarioPath);
}
