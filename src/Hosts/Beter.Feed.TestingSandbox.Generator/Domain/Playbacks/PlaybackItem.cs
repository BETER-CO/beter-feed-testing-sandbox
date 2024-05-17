using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;

public sealed record PlaybackItem
{
    public string PlaybackId { get; set; }
    public string InternalId { get; set; }
    public TestScenarioMessage Message { get; set; }
}
