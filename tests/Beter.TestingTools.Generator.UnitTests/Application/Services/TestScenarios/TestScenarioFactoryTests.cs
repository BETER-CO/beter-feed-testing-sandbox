using AutoFixture;
using Beter.TestingTools.Generator.Application.Services.TestScenarios;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.TestScenarios
{
    public class TestScenarioFactoryTests
    {
        private static readonly Fixture Fixture = new();
        private static readonly TestScenarioFactory Factory = new();

        [Fact]
        public async Task Create_WithValidStream_CreatesTestScenario()
        {
            // Arrange
            var caseId = Fixture.Create<int>();
            var streamContent = "{\"Description\":\"Description\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(streamContent));

            // Act
            var result = await Factory.Create(caseId, stream);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(caseId, result.CaseId);
            Assert.Equal("Description", result.Description); 
        }

        [Fact]
        public void Create_WithInvalidDirectory_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var invalidPath = "InvalidPath";

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(
                () => Factory.Create(invalidPath));
        }

        [Fact]
        public void Create_ReturnsTestScenarios_FromValidDirectory()
        {
            // Arrange
            var testScenarioPath = "Resources/ValidScenarios";

            var expected = new List<TestScenario>
            {
                new(){ CaseId = 1, Description = "Unit Test", Version = new Version("1.0"), Messages = new List<TestScenarioMessage>() },
                new(){ CaseId = 2, Description = "Unit Test", Version = new Version("1.1"), Messages = new List<TestScenarioMessage>() }
            };

            // Act
            var actual = Factory.Create(testScenarioPath);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Count());
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void Create_ThrowsDirectoryNotFoundException_WhenDirectoryNotFound()
        {
            // Arrange
            var testScenarioPath = "NonExistentDirectory";

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => Factory.Create(testScenarioPath));
        }

        [Fact]
        public void Create_ThrowsValidationException_WhenInvalidFileNameFormat()
        {
            // Arrange
            var testScenarioPath = "Resources/InvalidScenarioName";

            // Act & Assert
            Assert.Throws<ValidationException>(
                () => Factory.Create(testScenarioPath));
        }
    }
}
