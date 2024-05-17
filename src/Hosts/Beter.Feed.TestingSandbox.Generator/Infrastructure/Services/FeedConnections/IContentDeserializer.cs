namespace Beter.Feed.TestingSandbox.Generator.Infrastructure.Services.FeedConnections;

public interface IContentDeserializer
{
    TResponse DeserializeOrThrow<TResponse>(string content);
}
