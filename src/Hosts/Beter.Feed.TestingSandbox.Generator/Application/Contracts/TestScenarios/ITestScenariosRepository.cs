using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;

public interface ITestScenariosRepository
{
    IReadOnlyDictionary<int, TestScenario> GetAll();
    TestScenario Requre(int caseId);
    void AddOrUpdate(TestScenario scenario);
    void AddOrUpdateRange(IEnumerable<TestScenario> scenarios);
}
