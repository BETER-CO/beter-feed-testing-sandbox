using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Generator.Domain.TestScenarios;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public sealed class CustomOffsetTransformerStrategy : IOffsetTransformStrategy
    {
        public void Transform(TestScenarioMessage message, HubKind hubKind)
        {
            return;
        }
    }
}
