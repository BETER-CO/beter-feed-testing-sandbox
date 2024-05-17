using Microsoft.AspNetCore.Authorization;
using Beter.Feed.TestingSandbox.Models.Scoreboards;
using Beter.Feed.TestingSandbox.Emulator.Services;
using Beter.Feed.TestingSandbox.Emulator.Publishers;

namespace Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs.V1;

[AllowAnonymous]
public class ScoreboardHub : BaseHub<ScoreBoardModel, IFeedHub<ScoreBoardModel>>
{
    public ScoreboardHub(IMessagePublisher<ScoreBoardModel> publisher, IConnectionManager connectionManager, ILogger<BaseHub<ScoreBoardModel, IFeedHub<ScoreBoardModel>>> logger)
        : base(publisher, connectionManager, logger)
    {
    }
}