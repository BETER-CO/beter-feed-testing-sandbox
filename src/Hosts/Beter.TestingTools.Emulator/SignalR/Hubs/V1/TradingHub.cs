using Beter.TestingTools.Models.TradingInfos;
using Microsoft.AspNetCore.Authorization;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Services;

namespace Beter.TestingTools.Emulator.SignalR.Hubs.V1;

[AllowAnonymous]
public class TradingHub : BaseHub<TradingInfoModel, IFeedHub<TradingInfoModel>>
{
    public TradingHub(IMessagePublisher<TradingInfoModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<TradingInfoModel, IFeedHub<TradingInfoModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}
