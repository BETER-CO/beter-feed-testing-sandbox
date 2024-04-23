using Beter.TestingTools.Models.Scoreboards;
using Microsoft.AspNetCore.Authorization;
using Beter.TestingTools.Emulator.Publishers;
using Beter.TestingTools.Emulator.Services;

namespace Beter.TestingTools.Emulator.SignalR.Hubs.V1;

[AllowAnonymous]
public class ScoreboardHub : BaseHub<ScoreBoardModel, IFeedHub<ScoreBoardModel>>
{
    public ScoreboardHub(IMessagePublisher<ScoreBoardModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<ScoreBoardModel, IFeedHub<ScoreBoardModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}