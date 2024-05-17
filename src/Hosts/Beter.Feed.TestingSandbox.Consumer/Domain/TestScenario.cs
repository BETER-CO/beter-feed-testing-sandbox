namespace Beter.Feed.TestingSandbox.Consumer.Domain
{
    public sealed record TestScenario
    {
        public IEnumerable<TestScenarioMessage> Messages { get; init; }
    }
}
