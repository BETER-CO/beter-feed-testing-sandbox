namespace Beter.Feed.TestingSandbox.Generator.Infrastructure.Services.FeedConnections;

public interface IFeedEmulatorUrlProvider
{
    Uri DropConnection(string connectionId);

    Uri GetConnections();
}
