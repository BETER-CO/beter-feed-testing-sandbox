using Beter.Feed.TestingSandbox.Common.Models;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public interface IOffsetTransformStrategyResolver
    {
        IOffsetTransformStrategy Resolve(AdditionalInfo additionalInfo);
    }
}
