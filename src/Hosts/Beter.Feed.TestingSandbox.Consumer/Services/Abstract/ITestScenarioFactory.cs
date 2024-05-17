using Beter.Feed.TestingSandbox.Consumer.Domain;

namespace Beter.Feed.TestingSandbox.Consumer.Services.Abstract
{
    public interface ITestScenarioFactory
    {
        Task<TestScenario> Create(int caseId, Stream stream);
    }
}
