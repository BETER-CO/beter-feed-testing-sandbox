using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.Scoreboards;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;

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
                    model.Timer.TimeStamp = timestamp;
                });
        }
    }
}
