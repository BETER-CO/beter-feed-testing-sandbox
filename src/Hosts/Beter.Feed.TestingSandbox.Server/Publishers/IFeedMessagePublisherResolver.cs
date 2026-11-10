namespace Beter.Feed.TestingSandbox.Server.Publishers;

public interface IFeedMessagePublisherResolver
{
    IMessagePublisher Resolve(string channel);
}