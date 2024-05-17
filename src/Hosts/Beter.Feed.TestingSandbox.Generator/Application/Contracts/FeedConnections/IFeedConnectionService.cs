using Beter.Feed.TestingSandbox.Common.Models;

namespace Beter.Feed.TestingSandbox.Generator.Application.Contracts.FeedConnections;

public interface IFeedConnectionService
{
    public Task<IEnumerable<FeedConnection>> GetAsync(CancellationToken cancellationToken);

    public Task DropConnectionAsync(string connectionId, CancellationToken cancellationToken);
}