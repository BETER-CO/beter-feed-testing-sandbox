using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;

public sealed record PlaybackItem
{
    public Guid PlaybackId { get; set; }
    public Guid InternalId { get; set; }
    public TestScenarioMessage Message { get; set; }
}
