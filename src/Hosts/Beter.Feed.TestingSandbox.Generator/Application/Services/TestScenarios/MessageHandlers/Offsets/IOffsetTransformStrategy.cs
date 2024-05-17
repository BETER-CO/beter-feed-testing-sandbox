using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public interface IOffsetTransformStrategy
    {
        void Transform(TestScenarioMessage message, HubKind hubKind);
    }
}
