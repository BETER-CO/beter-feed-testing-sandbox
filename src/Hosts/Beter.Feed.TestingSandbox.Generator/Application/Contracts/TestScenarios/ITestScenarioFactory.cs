using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;

public interface ITestScenarioFactory
{
    Task<TestScenario> Create(int caseId, Stream stream);
    IEnumerable<TestScenario> Create(string testScenarioPath);
}
