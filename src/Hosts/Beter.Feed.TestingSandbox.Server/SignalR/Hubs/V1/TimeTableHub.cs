using Microsoft.AspNetCore.Authorization;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Server.Publishers;
using Beter.Feed.TestingSandbox.Server.Services.Connections;

namespace Beter.Feed.TestingSandbox.Server.SignalR.Hubs.V1;

[AllowAnonymous]
public class TimeTableHub : BaseHub<TimeTableItemModel, IFeedHub<TimeTableItemModel>>
{
    public TimeTableHub(IMessagePublisher<TimeTableItemModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<TimeTableItemModel, IFeedHub<TimeTableItemModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}
