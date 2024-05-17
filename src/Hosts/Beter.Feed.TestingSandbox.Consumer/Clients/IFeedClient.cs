namespace Beter.Feed.TestingSandbox.Consumer.Clients;

public interface IFeedClient
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
}