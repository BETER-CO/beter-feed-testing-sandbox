using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models;
using Beter.TestingTools.Generator.Domain.TestScenarios;
using Beter.TestingTools.Generator.Application.Extensions;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;

public class SubscriptionsRemovedTransformationRule : ITransformationRule
{
    public bool IsApplicable(TestScenarioMessage message) => message.MessageType == MessageTypes.SubscriptionsRemoved;

    public void Transform(MessagesTransformationContext context, TestScenarioMessage message)
    {
        message.Modify<SubscriptionsRemovedModel>(
            model => UpdateModel(model, message, context));
    }

    private static void UpdateModel(SubscriptionsRemovedModel model, TestScenarioMessage message, MessagesTransformationContext context)
    {
        UpdateModelId(model, context);
        UpdateScheduledAt(model, message, context);
    }

    private static void UpdateModelId(SubscriptionsRemovedModel model, MessagesTransformationContext context)
    {
        model.Ids = model.Ids.Select(oldMatchId => context.GetMatchProfile(oldMatchId).NewId).ToList();
    }

    private static void UpdateScheduledAt(
        SubscriptionsRemovedModel model,
        TestScenarioMessage message,
        MessagesTransformationContext context)
    {

        foreach (var matchId in model.Ids)
        {
            var pofile = context.GetMatchProfile(matchId);
            if (pofile.WasFirstTimeTableMessage && !pofile.IsFirstTimeTableMessageDelayProcessed)
            {
                message.ScheduledAt = message.ScheduledAt.ToUtcDateTime().Add(context.TimeOffsetAfterFirstTimetableMessageInSecounds).ToUnixTimeMilliseconds();
                pofile.IsFirstTimeTableMessageDelayProcessed = true;
            }
        }
    }
}
