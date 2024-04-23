using Confluent.Kafka;

namespace Beter.TestingTools.Emulator.Messaging;

public interface IConsumeMessageConverter
{
    bool CanProcess(ConsumeResult<byte[], byte[]> consumeResult);

    ConsumeMessageContext ConvertToMessageContextFromConsumeResult(ConsumeResult<byte[], byte[]> consumeResult);
}
