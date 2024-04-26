using Beter.TestingTools.Generator.Application.Common;
using Beter.TestingTools.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using System.Collections.Concurrent;

namespace Beter.TestingTools.Generator.Infrastructure.Repositories;

public sealed class InMemoryTestScenariosRepository : ITestScenariosRepository
{
    private readonly ConcurrentDictionary<int, TestScenario> _testScenarios = new();

    public InMemoryTestScenariosRepository(IEnumerable<TestScenario> scenarios)
    {
        var items = scenarios?.ToDictionary(
            scenario => scenario.CaseId,
            scenario => scenario) ?? new Dictionary<int, TestScenario>();

        _testScenarios = new ConcurrentDictionary<int, TestScenario>(items);
    }

    public void AddOrUpdate(TestScenario scenario)
    {
        if (scenario == null)
            throw new ArgumentNullException(nameof(scenario));

        _testScenarios[scenario.CaseId] = scenario;
    }

    public void AddOrUpdateRange(IEnumerable<TestScenario> scenarios)
    {
        if (scenarios == null)
            throw new ArgumentNullException(nameof(scenarios));

        foreach (var scenario in scenarios)
        {
            AddOrUpdate(scenario);
        }
    }

    public IReadOnlyDictionary<int, TestScenario> GetAll()
    {
        return _testScenarios;
    }

    public TestScenario Requre(int caseId)
    {
        if (!_testScenarios.TryGetValue(caseId, out var scenario))
            throw new RequiredEntityNotFoundException($"Required test scenario with case ID: '{caseId}' does not exist.");

        return scenario;
    }
}

