using AutoFixture;
using Beter.TestingTools.Generator.Application.Mappers;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.UnitTests.Fixtures;

namespace Beter.TestingTools.Generator.UnitTests.Application.Mappers
{
    public class TestScenarioMapperTests
    {
        private readonly Fixture _fixture;

        public TestScenarioMapperTests()
        {
            _fixture = new();
            _fixture.Customizations.Add(new JsonNodeBuilder());
        }

        [Fact]
        public void MapToDto_ReturnsCorrectTestScenarioDto()
        {
            // Arrange
            var source = _fixture.Create<TestScenario>();

            // Act
            var result = TestScenarioMapper.MapToDto(source);

            // Assert
            Assert.Equal(source.CaseId, result.CaseId);
            Assert.Equal(source.Description, result.Description);
            Assert.Equal(source.Version.ToString(), result.Version);
        }
    }
}
