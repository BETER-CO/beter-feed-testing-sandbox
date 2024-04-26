using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Domain.Playbacks;

public sealed record PlaybackItem
{
    public string PlaybackId { get; set; }
    public string InternalId { get; set; }
    public TestScenarioMessage Message { get; set; }
}
