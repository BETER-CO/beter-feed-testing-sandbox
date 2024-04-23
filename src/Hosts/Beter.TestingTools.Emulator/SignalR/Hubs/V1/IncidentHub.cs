using Beter.TestingTools.Models.Incidents;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Services;
using Microsoft.AspNetCore.Authorization;

namespace Beter.TestingTools.Emulator.SignalR.Hubs.V1;

[AllowAnonymous]
public class IncidentHub : BaseHub<IncidentModel, IFeedHub<IncidentModel>>
{
    public IncidentHub(IMessagePublisher<IncidentModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<IncidentModel, IFeedHub<IncidentModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}
