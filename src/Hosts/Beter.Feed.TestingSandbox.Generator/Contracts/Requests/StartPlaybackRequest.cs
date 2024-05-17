using System.Text.Json.Serialization;

namespace Beter.Feed.TestingSandbox.Generator.Contracts.Requests;

public record StartPlaybackRequest
{
    [JsonPropertyName("caseId")]
    public int CaseId { get; init; }

    [JsonPropertyName("timeOffsetInMinutes")]
    public int TimeOffsetInMinutes { get; init; }

    [JsonPropertyName("timeOffsetAfterFirstTimetableMessageInSecounds")]
    public int TimeOffsetAfterFirstTimetableMessageInSecounds { get; init; }
}
