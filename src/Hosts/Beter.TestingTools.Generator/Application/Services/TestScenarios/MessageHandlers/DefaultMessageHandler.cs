using Beter.TestingTools.Common.Constants;
using Beter.TestingTool.Generator.Application.Contracts;

namespace Beter.TestingTool.Generator.Application.Services.TestScenarios.MessageHandlers;

public class DefaultMessageHandler(IPublisher publisher) : BaseTestScenarioMessageHandler(publisher)
{
    public override bool IsApplicable(string messageType) => messageType == MessageTypes.SubscriptionsRemoved || messageType == MessageTypes.SystemEvent;
}

