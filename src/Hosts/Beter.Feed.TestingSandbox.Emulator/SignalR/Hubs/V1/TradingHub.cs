using Microsoft.AspNetCore.Authorization;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Services.Connections;

namespace Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs.V1;

[AllowAnonymous]
public class TradingHub : BaseHub<TradingInfoModel, IFeedHub<TradingInfoModel>>
{
    public TradingHub(IMessagePublisher<TradingInfoModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<TradingInfoModel, IFeedHub<TradingInfoModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}
