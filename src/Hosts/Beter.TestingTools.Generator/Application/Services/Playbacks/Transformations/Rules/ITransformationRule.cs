using Beter.TestingTool.Generator.Domain.TestScenarios;

namespace Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations.Rules;

public interface ITransformationRule
{
    bool IsApplicable(TestScenarioMessage message);
    void Transform(MessagesTransformationContext context, TestScenarioMessage message);
}