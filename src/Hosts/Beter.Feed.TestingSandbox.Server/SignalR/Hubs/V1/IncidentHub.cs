using Microsoft.AspNetCore.Authorization;
using Beter.Feed.TestingSandbox.Models.Incidents;
using Beter.Feed.TestingSandbox.Server.Publishers;
using Beter.Feed.TestingSandbox.Server.Services.Connections;

namespace Beter.Feed.TestingSandbox.Server.SignalR.Hubs.V1;

[AllowAnonymous]
public class IncidentHub : BaseHub<IncidentModel, IFeedHub<IncidentModel>>
{
    public IncidentHub(IMessagePublisher<IncidentModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<IncidentModel, IFeedHub<IncidentModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}
