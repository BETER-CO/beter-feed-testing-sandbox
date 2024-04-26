using Beter.TestingTools.Generator.Contracts.TestScenarios;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Mappers;

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
