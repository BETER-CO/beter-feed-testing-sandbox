namespace Beter.TestingTools.Emulator.Publishers;

public interface IFeedMessagePublisherResolver
{
    IMessagePublisher Resolve(string channel);
}