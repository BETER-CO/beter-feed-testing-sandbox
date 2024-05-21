using FluentValidation;
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

public class StartPlaybackRequestValidator : AbstractValidator<StartPlaybackRequest>
{
    public StartPlaybackRequestValidator()
    {
        RuleFor(request => request.CaseId)
            .GreaterThan(0)
            .WithMessage("CaseId must be greater than 0.");

        RuleFor(request => request.TimeOffsetInMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TimeOffsetInMinutes must be non-negative.");

        RuleFor(request => request.TimeOffsetAfterFirstTimetableMessageInSecounds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TimeOffsetAfterFirstTimetableMessageInSecounds must be non-negative.");
    }
}
