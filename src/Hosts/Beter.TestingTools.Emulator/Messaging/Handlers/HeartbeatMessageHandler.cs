using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models;
using Beter.TestingTools.Emulator.Messaging.Handlers.Abstract;
using Beter.TestingTools.Emulator.Publishers;

namespace Beter.TestingTools.Emulator.Messaging.Handlers;

public class HeartbeatMessageHandler : MessageHandlerBase<HeartbeatModel>
{
    private readonly IEnumerable<IMessagePublisher> _publishers;

    public HeartbeatMessageHandler(IEnumerable<IMessagePublisher> publishers)
    {
        _publishers = publishers ?? throw new ArgumentNullException(nameof(publishers));
    }

    public override async Task HandleAsync(HeartbeatModel message, ConsumeMessageContext context, CancellationToken cancellationToken = default)
    {
        var tasks = _publishers
                .AsParallel()
                .Select(s => s.SendGroupOnHeartbeatAsync(GroupNames.DefaultGroupName, cancellationToken))
                .ToArray();

        await Task.WhenAll(tasks);
    }
}