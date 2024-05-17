using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations.Rules;

public class TimeTableTransformationRule : ITransformationRule
{
    public bool IsApplicable(TestScenarioMessage message) => message.MessageType == MessageTypes.Timetable;

    public void Transform(MessagesTransformationContext context, TestScenarioMessage message)
    {
        message.Modify<IEnumerable<TimeTableItemModel>>(
            models => UpdateModel(models, message, context));
    }

    private static void UpdateModel(IEnumerable<TimeTableItemModel> models, TestScenarioMessage message, MessagesTransformationContext context)
    {
        foreach (var model in models)
        {
            UpdateStartDate(model, context);
            TransformationsExt.UpdateModelId(model, context);
            TransformationsExt.UpdateScheduledAt(model, message, context);
            TransformationsExt.UpdateTimestampAndDate(
                model,
                context,
                model => model.Timestamp,
                (model, timestamp) => model.Timestamp = timestamp);
        }
    }

    private static void UpdateStartDate(TimeTableItemModel model, MessagesTransformationContext context)
    {
        var matchId = model.Id;
        var profile = context.GetMatchProfile(matchId);

        if (model.StartDate != profile.OldStartDate)
        {
            profile.NewStartDate = RescheduledStartDate(profile.NewStartDate, model.StartDate.Value, profile.OldStartDate, context.AccelerationFactor).UtcDateTime;
            profile.OldStartDate = model.StartDate.Value;
        }

        model.StartDate = profile.NewStartDate;
    }

    private static DateTimeOffset RescheduledStartDate(DateTimeOffset currentStartDate, DateTime rescheduledStartDate, DateTimeOffset oldStartDate, double accelerationFactor)
    {
        return currentStartDate + (rescheduledStartDate - oldStartDate) / accelerationFactor;
    }
}
