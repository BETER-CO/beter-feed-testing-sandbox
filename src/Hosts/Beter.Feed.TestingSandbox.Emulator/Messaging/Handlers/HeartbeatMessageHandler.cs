using Beter.Feed.TestingSandbox.Common.Constants;
using Beter.Feed.TestingSandbox.Models;
using Beter.Feed.TestingSandbox.Emulator.Messaging;
using Beter.Feed.TestingSandbox.Emulator.Publishers;
using Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers.Abstract;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Handlers;

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