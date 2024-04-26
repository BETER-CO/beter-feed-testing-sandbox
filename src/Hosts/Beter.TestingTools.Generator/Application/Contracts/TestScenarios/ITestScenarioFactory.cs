using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioFactory
{
    Task<TestScenario> Create(int caseId, Stream stream);
    IEnumerable<TestScenario> Create(string testScenarioPath);
}
