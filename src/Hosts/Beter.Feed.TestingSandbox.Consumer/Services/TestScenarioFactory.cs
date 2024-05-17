using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Consumer.Domain;
using Beter.Feed.TestingSandbox.Consumer.Services.Abstract;
using System.Text.Json;

namespace Beter.Feed.TestingSandbox.Consumer.Services
{
    public sealed class TestScenarioFactory : ITestScenarioFactory
    {
        public async Task<TestScenario> Create(int caseId, Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            var content = await streamReader.ReadToEndAsync();

            try
            {
                return JsonHubSerializer.Deserialize<TestScenario>(content);
            }
            catch (JsonException e)
            {
                throw new ArgumentException("The content of the test scenario file is invalid JSON.", e);
            }
        }
    }
}
