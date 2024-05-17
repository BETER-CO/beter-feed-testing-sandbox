using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations.Rules;

public interface ITransformationRule
{
    bool IsApplicable(TestScenarioMessage message);
    void Transform(MessagesTransformationContext context, TestScenarioMessage message);
}