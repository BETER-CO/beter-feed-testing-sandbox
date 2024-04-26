using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Generator.Application.Contracts;

namespace Beter.TestingTools.Generator.Application.Services.TestScenarios.MessageHandlers;

public class DefaultMessageHandler(IPublisher publisher) : BaseTestScenarioMessageHandler(publisher)
{
    public override bool IsApplicable(string messageType) => messageType == MessageTypes.SubscriptionsRemoved || messageType == MessageTypes.SystemEvent;
}

