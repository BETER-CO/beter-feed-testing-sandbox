using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;
using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations;

public interface ITransformationManager
{
    IEnumerable<TestScenarioMessage> ApplyTransformation(MessagesTransformationContext context, IEnumerable<TestScenarioMessage> messages);
}

public class TransformationManager : ITransformationManager
{
    private readonly IEnumerable<ITransformationRule> _transformationRules;

    public TransformationManager(IEnumerable<ITransformationRule> transformationRules)
    {
        _transformationRules = transformationRules ?? throw new ArgumentNullException(nameof(transformationRules));
    }

    public IEnumerable<TestScenarioMessage> ApplyTransformation(MessagesTransformationContext context, IEnumerable<TestScenarioMessage> messages)
    {
        foreach (var message in messages)
        {
            TransformationsExt.UpdateScheduledAt(message, context);

            var transformationRule = _transformationRules.SingleOrDefault(rule => rule.IsApplicable(message));
            if (transformationRule != null)
            {
                transformationRule.Transform(context, message);
            }
        }

        return messages;
    }
}
