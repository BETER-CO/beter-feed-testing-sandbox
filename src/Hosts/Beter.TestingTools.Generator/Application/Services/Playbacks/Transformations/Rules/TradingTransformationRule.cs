using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.TradingInfos;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;

public class TradingTransformationRule : ITransformationRule
{
    public bool IsApplicable(TestScenarioMessage message) => message.MessageType == MessageTypes.Trading;

    public void Transform(MessagesTransformationContext context, TestScenarioMessage message)
    {
        message.Modify<IEnumerable<TradingInfoModel>>(
            models => UpdateModel(models, message, context));
    }

    private static void UpdateModel(IEnumerable<TradingInfoModel> models, TestScenarioMessage message, MessagesTransformationContext context)
    {
        foreach (var model in models)
        {
            TransformationsExt.UpdateModelId(model, context);
            TransformationsExt.UpdateScheduledAt(model, message, context);
            TransformationsExt.UpdateTimestampAndDate(
                model,
                context,
                model => model.Timestamp,
                (model, timestamp) => model.Timestamp = timestamp);
        }
    }
}
