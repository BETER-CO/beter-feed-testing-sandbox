using Beter.TestingTools.Models;

namespace Beter.TestingTools.Emulator.SignalR.Hubs;

public interface IFeedHub<in TModel> where TModel : class
{
    Task OnUpdate(IEnumerable<TModel> items, CancellationToken cancellationToken);
    Task OnHeartbeat(long keepalive, CancellationToken cancellationToken);
    Task OnSubscriptionsRemove(string[] ids, CancellationToken cancellationToken);
    Task OnSystemEvent(IEnumerable<IGlobalMessageModel> item, CancellationToken cancellationToken);
}
