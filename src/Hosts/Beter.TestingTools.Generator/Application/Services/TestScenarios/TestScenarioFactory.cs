using Beter.TestingTool.Generator.Application.Contracts.TestScenarios;
using Beter.TestingTool.Generator.Domain.TestScenarios;
using System.Reflection;
using System.Text.Json;

namespace Beter.TestingTool.Generator.Application.Services.TestScenarios;

public sealed class TestScenarioFactory : ITestScenarioFactory
{
    public async Task<TestScenario> Create(int caseId, Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();

        var testScenario = JsonSerializer.Deserialize<TestScenario>(content);

        return testScenario with
        {
            CaseId = caseId
        };
    }

    public IEnumerable<TestScenario> Create(string testScenarioPath) =>
        ReadTestScenarioFromAssembly(testScenarioPath);

    private IEnumerable<TestScenario> ReadTestScenarioFromAssembly(string testScenarioPath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var absolutePath = Path.GetDirectoryName(assembly.Location);
        var directoryPath = Path.Combine(absolutePath, testScenarioPath);

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"The directory {testScenarioPath} was not found in the assembly.");
        }

        foreach (var scenario in Directory.GetFiles(directoryPath))
        {
            yield return CreateTestScenario(scenario);
        }
    }

    //TODO Add validator for json content
    private TestScenario CreateTestScenario(string scenarioPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(scenarioPath);
        if (!int.TryParse(fileName, out var caseId))
            throw new InvalidOperationException("Invalid file name format.");

        var jsonContent = File.ReadAllText(scenarioPath);
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var scenario = JsonSerializer.Deserialize<TestScenario>(jsonContent, jsonSerializerOptions);
        scenario.SetCaseId(caseId);

        return scenario;
    }
}
