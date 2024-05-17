using Beter.Feed.TestingSandbox.Generator.Contracts.Requests;

namespace Beter.Feed.TestingSandbox.Generator.Contracts.Responses;

public sealed class StopPlaybackResponse
{
    public StopPlaybackCommand Command { get; set; }

    public IEnumerable<StopPlaybackItemResponse> Items { get; set; }
}

public sealed class StopPlaybackItemResponse
{
    public string PlaybackId { get; set; }

    public int TestCaseId { get; init; }

    public string Version { get; init; }

    public string Description { get; init; }
}
