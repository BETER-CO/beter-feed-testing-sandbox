using Beter.TestingTool.Generator.Contracts.TestScenarios;
using TestScenario = Beter.TestingTool.Generator.Domain.TestScenarios.TestScenario;

namespace Beter.TestingTool.Generator.Application.Mappers;

public static class TestScenarioMapper
{
    public static TestScenarioDto MapToDto(TestScenario source)
    {
        return new TestScenarioDto
        {
            CaseId = source.CaseId,
            Description = source.Description,
            Version = source.Version.ToString()
        };
    }
}
