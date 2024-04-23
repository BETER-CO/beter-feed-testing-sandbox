using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Contracts.TestScenarios;

public interface ITestScenariosRepository
{
    IReadOnlyDictionary<int, TestScenario> GetAll();
    TestScenario Requre(int caseId);
    void Add(TestScenario scenario);
    void AddRange(IEnumerable<TestScenario> scenarios);
}
