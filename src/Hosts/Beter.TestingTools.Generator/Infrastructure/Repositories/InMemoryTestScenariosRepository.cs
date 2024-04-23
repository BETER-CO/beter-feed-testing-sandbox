using Beter.TestingTool.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Domain.TestScenarios;
using System.Collections.Concurrent;

namespace Beter.TestingTool.Generator.Infrastructure.Repositories;

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

    public void Add(TestScenario scenario)
    {
        if (scenario == null)
            throw new ArgumentNullException(nameof(scenario));

        if (_testScenarios.ContainsKey(scenario.CaseId))
            throw new ArgumentException($"Test scenario with case ID {scenario.CaseId} already exists.");

        _testScenarios[scenario.CaseId] = scenario;
    }

    public void AddRange(IEnumerable<TestScenario> scenarios)
    {
        if (scenarios == null)
            throw new ArgumentNullException(nameof(scenarios));

        foreach (var scenario in scenarios)
        {
            Add(scenario);
        }
    }

    public IReadOnlyDictionary<int, TestScenario> GetAll()
    {
        return _testScenarios;
    }

    public TestScenario Requre(int caseId)
    {
        if (!_testScenarios.TryGetValue(caseId, out var scenario))
        {
            throw new InvalidOperationException($"Test scenario with case ID '{caseId}' does not exist.");
        }

        return scenario;
    }
}

