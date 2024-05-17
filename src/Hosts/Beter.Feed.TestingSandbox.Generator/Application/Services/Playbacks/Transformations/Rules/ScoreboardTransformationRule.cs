using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;
using Beter.Feed.TestingSandbox.Models.Scoreboards;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations.Rules;

public class ScoreboardTransformationRule : ITransformationRule
{
    public bool IsApplicable(TestScenarioMessage message) => message.MessageType == MessageTypes.Scoreboard;

    public void Transform(MessagesTransformationContext context, TestScenarioMessage message)
    {
        message.Modify<IEnumerable<ScoreBoardModel>>(
            models => UpdateModel(models, message, context));
    }

    private static void UpdateModel(IEnumerable<ScoreBoardModel> models, TestScenarioMessage message, MessagesTransformationContext context)
    {
        foreach (var model in models)
        {
            TransformationsExt.UpdateModelId(model, context);
            TransformationsExt.UpdateScheduledAt(model, message, context);
            TransformationsExt.UpdateTimestampAndDate(
                model,
                context,
                model => model.Timestamp,
                (model, timestamp) =>
                {
                    model.Timestamp = timestamp;

                    if (model.Timer != null)
                    {
                        model.Timer.TimeStamp = timestamp;
                    }
                });
        }
    }
}
