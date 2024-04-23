namespace Beter.TestingTool.Generator.Infrastructure.Services.FeedConnections;

public interface IContentDeserializer
{
    TResponse DeserializeOrThrow<TResponse>(string content);
}
