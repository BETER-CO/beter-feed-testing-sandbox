using Beter.TestingTools.Generator.Application.Exceptions;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Infrastructure.Repositories;

namespace Beter.TestingTools.Generator.UnitTests.Infrastructure.Repositories
{
    public class InMemoryTestScenariosRepositoryTests
    {
        private readonly InMemoryTestScenariosRepository _repository;

        public InMemoryTestScenariosRepositoryTests()
        {
            _repository = new InMemoryTestScenariosRepository(new List<TestScenario>());
        }

        [Fact]
        public void AddOrUpdate_AddsScenarioCorrectly()
        {
            // Arrange
            var scenario1 = new TestScenario { CaseId = 1 };
            var scenario2 = new TestScenario { CaseId = 2 };

            // Act
            _repository.AddOrUpdate(scenario1);
            _repository.AddOrUpdate(scenario2);

            // Assert
            var retrievedScenario1 = _repository.Requre(1);
            Assert.Equal(scenario1, retrievedScenario1);

            var retrievedScenario2 = _repository.Requre(2);
            Assert.Equal(scenario2, retrievedScenario2);

            var count = _repository.GetAll().Values.Count();
            Assert.Equal(2, count);
        }

        [Fact]
        public void AddOrUpdate_ShoudUpdateScenario_WhenScenarioWithSameCaseIdAlreadyExists()
        {
            // Arrange
            var scenario1 = new TestScenario { CaseId = 1, Description = "Scenario 1" };
            var scenario2 = new TestScenario { CaseId = 1, Description = "Scenario 2" };
            _repository.AddOrUpdate(scenario1);

            // Act
            _repository.AddOrUpdate(scenario2);

            // Assert
            Assert.Equal(1, _repository.GetAll().Count);
            Assert.Equal(scenario2, _repository.Requre(1));
        }

        [Fact]
        public void Add_ThrowsException_WhenScenarioIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => _repository.AddOrUpdate(null));
        }

        [Fact]
        public void Requre_ReturnsScenarioCorrectly()
        {
            // Arrange
            var scenarios = new List<TestScenario>
            {
                new TestScenario { CaseId = 1 },
                new TestScenario { CaseId = 2 }
            };
            var repository = new InMemoryTestScenariosRepository(scenarios);

            // Act
            var retrievedScenario = repository.Requre(2);

            // Assert
            Assert.Equal(scenarios[1], retrievedScenario);
        }

        [Fact]
        public void Requre_RequiredEntityNotFoundException_WhenScenarioDoesNotExist()
        {
            // Act & Assert
            Assert.Throws<RequiredEntityNotFoundException>(
                () => _repository.Requre(1));
        }

        [Fact]
        public void GetAll_ReturnsAllScenarios()
        {
            // Arrange
            var scenarios = new List<TestScenario>
            {
                new TestScenario { CaseId = 1 },
                new TestScenario { CaseId = 2 }
            };
            var repository = new InMemoryTestScenariosRepository(scenarios);

            // Act
            var allScenarios = repository.GetAll();

            // Assert
            Assert.Equal(scenarios.Count, allScenarios.Count);
            foreach (var scenario in scenarios)
            {
                Assert.Contains(scenario.CaseId, allScenarios.Keys);
                Assert.Contains(scenario, allScenarios.Values);
            }
        }

        [Fact]
        public void AddOrUpdateRange_ThrowsArgumentNullException_WhenScenariosIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => _repository.AddOrUpdateRange(null));
        }

        [Fact]
        public void AddOrUpdateRange_AddsScenarios_WhenValidScenariosProvided()
        {
            // Arrange
            var scenario1 = new TestScenario { CaseId = 1, Description = "Scenario 1" };
            var scenario2 = new TestScenario { CaseId = 2, Description = "Scenario 2" };
            var scenarios = new List<TestScenario> { scenario1, scenario2 };

            // Act
            _repository.AddOrUpdateRange(scenarios);

            // Assert
            Assert.Equal(scenario1, _repository.Requre(1));
            Assert.Equal(scenario2, _repository.Requre(2));
        }
    }
}
