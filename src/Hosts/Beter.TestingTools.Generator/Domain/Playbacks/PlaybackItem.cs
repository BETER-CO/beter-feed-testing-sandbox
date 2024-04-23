using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Domain.Playbacks;

public sealed record PlaybackItem
{
    public string PlaybackId { get; set; }
    public string InternalId { get; set; }
    public TestScenarioMessage Message { get; set; }
}
