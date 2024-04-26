namespace Beter.TestingTools.Generator.Infrastructure.Services.FeedConnections;

public interface IFeedEmulatorUrlProvider
{
    Uri DropConnection(string connectionId);

    Uri GetConnections();
}
