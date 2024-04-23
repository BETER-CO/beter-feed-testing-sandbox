using Beter.TestingTools.Models.TimeTableItems;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Services;
using Microsoft.AspNetCore.Authorization;

namespace Beter.TestingTools.Emulator.SignalR.Hubs.V1;

[AllowAnonymous]
public class TimeTableHub : BaseHub<TimeTableItemModel, IFeedHub<TimeTableItemModel>>
{
    public TimeTableHub(IMessagePublisher<TimeTableItemModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<TimeTableItemModel, IFeedHub<TimeTableItemModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}
