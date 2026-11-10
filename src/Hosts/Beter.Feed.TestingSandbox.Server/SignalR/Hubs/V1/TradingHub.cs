using Microsoft.AspNetCore.Authorization;
using Beter.Feed.TestingSandbox.Models.TradingInfos;
using Beter.Feed.TestingSandbox.Server.Publishers;
using Beter.Feed.TestingSandbox.Server.Services.Connections;

namespace Beter.Feed.TestingSandbox.Server.SignalR.Hubs.V1;

[AllowAnonymous]
public class TradingHub : BaseHub<TradingInfoModel, IFeedHub<TradingInfoModel>>
{
    public TradingHub(IMessagePublisher<TradingInfoModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<TradingInfoModel, IFeedHub<TradingInfoModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}
