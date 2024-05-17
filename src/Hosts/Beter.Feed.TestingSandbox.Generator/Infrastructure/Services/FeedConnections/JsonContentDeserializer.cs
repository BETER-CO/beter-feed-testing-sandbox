using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Beter.Feed.TestingSandbox.Generator.Infrastructure.Services.FeedConnections;

public class JsonContentDeserializer : IContentDeserializer
{
    private readonly JsonSerializerSettings _serializerOptions = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public TResponse DeserializeOrThrow<TResponse>(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content, nameof(content));

        TResponse deserialized;
        try
        {
            deserialized = JsonConvert.DeserializeObject<TResponse>(content, _serializerOptions);
        }
        catch (JsonException e)
        {
            throw new ArgumentException("Content is invalid JSON.", nameof(content), e);
        }

        if (deserialized == null)
            throw new ArgumentException("Content is invalid JSON.", nameof(content));

        return deserialized;
    }
}
