using Beter.Feed.TestingSandbox.Generator.Contracts.TestScenarios;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Mappers;

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
