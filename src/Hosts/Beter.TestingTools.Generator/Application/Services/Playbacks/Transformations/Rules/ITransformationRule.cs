using Beter.TestingTools.Generator.Domain.TestScenarios;

namespace Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Rules;

public interface ITransformationRule
{
    bool IsApplicable(TestScenarioMessage message);
    void Transform(MessagesTransformationContext context, TestScenarioMessage message);
}