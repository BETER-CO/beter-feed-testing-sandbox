using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models.Incidents;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;

public class IncidentTransformationRule : ITransformationRule
{
    public bool IsApplicable(TestScenarioMessage message) => message.MessageType == MessageTypes.Incident;

    public void Transform(MessagesTransformationContext context, TestScenarioMessage message)
    {
        message.Modify<IEnumerable<IncidentModel>>(
            models => UpdateModel(models, message, context));
    }

    private static void UpdateModel(IEnumerable<IncidentModel> models, TestScenarioMessage message, MessagesTransformationContext context)
    {
        foreach (var model in models)
        {
            TransformationsExt.UpdateModelId(model, context);
            TransformationsExt.UpdateScheduledAt(model, message, context);
            TransformationsExt.UpdateTimestampAndDate(
                model,
                context,
                model => model.Date,
                (model, dateTime) => model.Date = dateTime);
        }
    }
}

