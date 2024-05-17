using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Generator.Application.Contracts;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers;

public class DefaultMessageHandler(IPublisher publisher) : BaseTestScenarioMessageHandler(publisher)
{
    public override bool IsApplicable(string messageType) => messageType == MessageTypes.SubscriptionsRemoved || messageType == MessageTypes.SystemEvent;
}

