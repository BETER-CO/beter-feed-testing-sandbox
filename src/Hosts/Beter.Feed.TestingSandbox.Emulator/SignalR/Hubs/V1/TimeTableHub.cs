using Microsoft.AspNetCore.Authorization;
using Beter.Feed.TestingSandbox.Models.TimeTableItems;
using Beter.Feed.TestingSandbox.Emulator.Services;
using Beter.Feed.TestingSandbox.Emulator.Publishers;

namespace Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs.V1;

[AllowAnonymous]
public class TimeTableHub : BaseHub<TimeTableItemModel, IFeedHub<TimeTableItemModel>>
{
    public TimeTableHub(IMessagePublisher<TimeTableItemModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<TimeTableItemModel, IFeedHub<TimeTableItemModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}
