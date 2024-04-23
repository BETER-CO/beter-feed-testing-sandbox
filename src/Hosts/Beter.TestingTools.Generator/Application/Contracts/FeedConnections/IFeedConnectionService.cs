using Beter.TestingTools.Common.Models;

namespace Beter.TestingTool.Generator.Application.Contracts.FeedConnections;

public interface IFeedConnectionService
{
    public Task<IEnumerable<FeedConnection>> GetAsync(CancellationToken cancellationToken);

    public Task DropConnectionAsync(string connectionId, CancellationToken cancellationToken);
}