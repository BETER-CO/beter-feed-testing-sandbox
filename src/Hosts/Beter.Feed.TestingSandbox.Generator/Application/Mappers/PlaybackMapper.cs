﻿using Beter.Feed.TestingSandbox.Generator.Contracts.Playbacks;
using Beter.Feed.TestingSandbox.Generator.Contracts.Responses;
using Beter.Feed.TestingSandbox.Generator.Domain.Playbacks;

namespace Beter.Feed.TestingSandbox.Generator.Application.Mappers;

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

