namespace Beter.Feed.TestingSandbox.Emulator.Publishers;

public interface IFeedMessagePublisherResolver
{
    IMessagePublisher Resolve(string channel);
}