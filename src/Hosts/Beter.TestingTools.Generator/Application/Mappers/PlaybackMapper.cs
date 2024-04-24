using Beter.TestingTool.Generator.Contracts.Playbacks;
using Beter.TestingTool.Generator.Contracts.Responses;
using Beter.TestingTool.Generator.Domain.Playbacks;

namespace Beter.TestingTool.Generator.Application.Mappers;

public static class PlaybackMapper
{
    public static PlaybackDto MapToDto(Playback source)
    {
        return new PlaybackDto
        {
            PlaybackId = source.Id,
            CaseId = source.CaseId,
            Description = source.Description,
            Version = source.Version.ToString(),
            StartedAt = source.StartedAt,
            ActiveMessagesCount = source.ActiveMessagesCount,
            LastMessageSentAt = source.LastMessageSentAt
        };
    }

    public static StopPlaybackItemResponse MapToStopPlaybackItemResponse(Playback source)
    {
        return new StopPlaybackItemResponse
        {
            PlaybackId = source.Id,
            TestCaseId = source.CaseId,
            Description = source.Description,
            Version = source.Version.ToString()
        };
    }
}

