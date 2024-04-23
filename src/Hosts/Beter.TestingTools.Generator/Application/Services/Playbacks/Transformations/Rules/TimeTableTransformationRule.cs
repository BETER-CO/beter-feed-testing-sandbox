using Beter.TestingTool.Generator.Application.Extensions;
using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.TimeTableItems;
using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations.Rules;

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
        var newStartDate = CalculateNewStartDateForEachMatchId(context);

        foreach (var model in models)
        {
            UpdateStartDate(model, context, newStartDate);
            TransformationsExt.UpdateModelId(model, context);
            TransformationsExt.UpdateScheduledAt(model, message, context);
            TransformationsExt.UpdateTimestampAndDate(
                model,
                context,
                model => model.Timestamp.ToUtcDateTime(),
                (model, dateTime) => model.Timestamp = dateTime.ToUnixTimeMilliseconds());
        }
    }

    private static void UpdateStartDate(TimeTableItemModel model, MessagesTransformationContext context, Dictionary<string, DateTime> newStartDate)
    {
        var matchId = model.Id;
        var profile = context.GetMatchProfile(matchId);

        if (model.StartDate != profile.OldStartDate)
        {
            newStartDate[matchId] = RescheduledStartDate(newStartDate[matchId], model.StartDate.Value, profile.OldStartDate, context.AccelerationFactor).UtcDateTime;
            profile.OldStartDate = model.StartDate.Value;
        }

        model.StartDate = newStartDate[matchId];
    }

    private static DateTimeOffset RescheduledStartDate(DateTimeOffset currentStartDate, DateTime rescheduledStartDate, DateTimeOffset oldStartDate, double accelerationFactor)
    {
        return currentStartDate + (rescheduledStartDate - oldStartDate) / accelerationFactor;
    }

    private static Dictionary<string, DateTime> CalculateNewStartDateForEachMatchId(MessagesTransformationContext context)
    {
        return context.Matches.ToDictionary(
             kv => kv.Key,
             kv => CalculateNewStartDate(context, context.GetMatchProfile(kv.Key).OldStartDate));
    }

    private static DateTime CalculateNewStartDate(MessagesTransformationContext context, DateTime oldStartDate)
    {
        return oldStartDate
            + (context.NewFirstMessageScheduledAt - oldStartDate)
            + (oldStartDate - context.OldFirstMessageScheduledAt) / context.AccelerationFactor
            + context.TimeOffsetAfterFirstTimetableMessageInSecounds;
    }
}
