using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.GlobalEvents;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;

public class SystemEventTransformationRule : ITransformationRule
{
    public bool IsApplicable(TestScenarioMessage message) => message.MessageType == MessageTypes.SystemEvent;

    public void Transform(MessagesTransformationContext context, TestScenarioMessage message)
    {
        message.Modify<IEnumerable<GlobalMessageModel>>(
            models => UpdateModel(models, message, context));
    }

    private static void UpdateModel(IEnumerable<GlobalMessageModel> models, TestScenarioMessage message, MessagesTransformationContext context)
    {
        foreach (var model in models)
        {
            TransformationsExt.UpdateModelId(model, context);
            TransformationsExt.UpdateScheduledAt(model, message, context);
        }
    }
}
