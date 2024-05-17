namespace Beter.Feed.TestingSandbox.Consumer.Domain
{
    public sealed class TestScenarioTemplateMissmatchItem
    {
        public string Actual { get; set; }
        public string Expected { get; set; }

        public TestScenarioTemplateMissmatchItem(string expected, string actual)
        {
            Actual = actual ?? throw new ArgumentNullException(nameof(actual));
            Expected = expected ?? throw new ArgumentNullException(nameof(expected));
        }
    }
}
