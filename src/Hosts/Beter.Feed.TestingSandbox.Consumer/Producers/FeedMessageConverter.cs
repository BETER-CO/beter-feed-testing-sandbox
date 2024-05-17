using Beter.Feed.TestingSandbox.Common;
using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Common.Serialization;
using Beter.Feed.TestingSandbox.Consumer.Models;
using Confluent.Kafka;
using System.Text;

namespace Beter.Feed.TestingSandbox.Consumer.Producers;

internal class FeedMessageConverter<TModel> : IProducerMessageConverter<FeedMessageModel<TModel>>
{
    public Message<string, byte[]> ConvertToKafkaMessage(FeedMessageModel<TModel> message)
    {
        var headers = new Headers
        {
            { HeaderNames.MessageType, Encoding.UTF8.GetBytes(TestingToolsMetadata.GetMessageType(typeof(TModel))) },
            { HeaderNames.MessageChannel, Encoding.UTF8.GetBytes(message.Channel) },
            { HeaderNames.MessageMethod, Encoding.UTF8.GetBytes(message.Method) }
        };

        return new Message<string, byte[]>
        {
            Key = message.Method,
            Value = ConvertData(message.Value),
            Headers = headers,
            Timestamp = new Timestamp(DateTimeOffset.UtcNow)
        };
    }
    private static byte[] ConvertData(TModel messages) =>
        Encoding.UTF8.GetBytes(
            JsonHubSerializer.Serialize(messages));
}

