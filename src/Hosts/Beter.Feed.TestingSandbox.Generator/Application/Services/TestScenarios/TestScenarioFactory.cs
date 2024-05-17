using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios;

public sealed class TestScenarioFactory : ITestScenarioFactory
{
    public async Task<TestScenario> Create(int caseId, Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();

        try
        {
            var testScenario = JsonHubSerializer.Deserialize<TestScenario>(content);

            return testScenario with
            {
                CaseId = caseId
            };
        }
        catch (JsonException e)
        {
            throw new InvalidFileContentException("The content of the test scenario file is invalid JSON.", e);
        }
    }

    public IEnumerable<TestScenario> Create(string testScenarioPath)
    {
        return ReadTestScenarioFromAssembly(testScenarioPath);
    }

    private IEnumerable<TestScenario> ReadTestScenarioFromAssembly(string testScenarioPath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var absolutePath = Path.GetDirectoryName(assembly.Location);
        var directoryPath = Path.Combine(absolutePath, testScenarioPath);

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"The directory {testScenarioPath} was not found in the assembly.");
        }

        var result = new List<TestScenario>();
        foreach (var scenario in Directory.GetFiles(directoryPath))
        {
            result.Add(CreateTestScenario(scenario));
        }

        return result;
    }

    //TODO Add validator for json content
    private TestScenario CreateTestScenario(string scenarioPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(scenarioPath);
        if (!int.TryParse(fileName, out var caseId))
            throw new ValidationException("Invalid file name format. File name should be in the format of a number, for example, '1.json'.");

        var jsonContent = File.ReadAllText(scenarioPath);

        try
        {
            var scenario = JsonHubSerializer.Deserialize<TestScenario>(jsonContent);
            scenario.SetCaseId(caseId);

            return scenario;
        }
        catch (JsonException e)
        {
            throw new InvalidFileContentException("The content of the test scenario file is invalid JSON.", e);
        }
    }

    public class InvalidFileContentException : ValidationException
    {
        public InvalidFileContentException()
        {
        }

        public InvalidFileContentException(string message) : base(message)
        {
        }

        public InvalidFileContentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
