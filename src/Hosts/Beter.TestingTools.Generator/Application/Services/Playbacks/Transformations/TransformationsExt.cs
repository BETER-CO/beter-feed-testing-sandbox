using Beter.TestingTools.Common;
using Beter.TestingTools.Models;
using Beter.TestingTool.Generator.Application.Contracts.Playbacks;
using Beter.TestingTool.Generator.Application.Extensions;
using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations;

public static class TransformationsExt
{
    public static void UpdateModelId<TModel>(TModel model, MessagesTransformationContext context)
       where TModel : IIdentityModel
    {
        model.Id = context.GetMatchProfile(model.Id).NewId;
    }

    public static void UpdateScheduledAt<TModel>(
        TModel model,
        TestScenarioMessage message,
        MessagesTransformationContext context) where TModel : IIdentityModel
    {
        var profile = context.GetMatchProfile(model.Id);
        if (profile.WasFirstTimeTableMessage && !profile.IsFirstTimeTableMessageDelayProcessed)
        {
            message.ScheduledAt = message.ScheduledAt.ToUtcDateTime().Add(context.TimeOffsetAfterFirstTimetableMessageInSecounds).ToUnixTimeMilliseconds();
            profile.IsFirstTimeTableMessageDelayProcessed = true;
        }
    }

    public static void UpdateScheduledAt(
        TestScenarioMessage message,
        MessagesTransformationContext context)
    {
        if (context.ReplyMode == ReplyMode.HistoricalTimeline)
        {
            var diffScheduledAtBetweenMessages = DateTimeOffset.FromUnixTimeMilliseconds(message.ScheduledAt).UtcDateTime.Subtract(context.OldFirstMessageScheduledAt);
            var currentScheduledAt = context.NewFirstMessageScheduledAt.Add(diffScheduledAtBetweenMessages);
            message.ScheduledAt = currentScheduledAt.ToUnixTimeMilliseconds();
        }
        else
        {
            throw new NotImplementedException("Does not support in current version of testing tools.");
        }
    }

    public static void UpdateTimestampAndDate<TModel>(
        TModel model,
        MessagesTransformationContext context,
        Func<TModel, DateTime> timestampGetter,
        Action<TModel, DateTime> timestampSetter) where TModel : IIdentityModel
    {
        if (context.ReplyMode == ReplyMode.HistoricalTimeline)
        {
            var oldFirstMessageTimestamp = context.GetMatchProfile(model.Id).OldFirstTimestampByEachMessageType[TestingToolsMetadata.ToMessageType<TModel>()];
            var diffTimestampBetweenMessages = timestampGetter(model).Subtract(oldFirstMessageTimestamp);
            var currentTimestamp = context.TestCaseStart.Add(diffTimestampBetweenMessages);

            timestampSetter(model, currentTimestamp);
        }
        else
        {
            throw new NotImplementedException("Does not support in current version of testing tools.");
        }
    }

    public static void UpdateTimestampAndDate<TModel>(
        TModel model,
        MessagesTransformationContext context,
        Func<TModel, long> timestampGetter,
        Action<TModel, long> timestampSetter) where TModel : IIdentityModel
    {
        if (context.ReplyMode == ReplyMode.HistoricalTimeline)
        {
            var oldFirstMessageTimestamp = context.GetMatchProfile(model.Id).OldFirstTimestampByEachMessageType[TestingToolsMetadata.ToMessageType<TModel>()];
            var diffTimestampBetweenMessages = timestampGetter(model).ToUtcDateTime().Subtract(oldFirstMessageTimestamp);
            var currentTimestamp = context.TestCaseStart.Add(diffTimestampBetweenMessages);

            timestampSetter(model, currentTimestamp.ToUnixTimeMilliseconds());
        }
        else
        {
            throw new NotImplementedException("Does not support in current version of testing tools.");
        }
    }
}
