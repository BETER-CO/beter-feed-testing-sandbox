using Beter.Feed.TestingSandbox.Common.Enums;
using Beter.Feed.TestingSandbox.Models;

namespace Beter.Feed.TestingSandbox.Emulator.Publishers;

public interface IMessagePublisher
{
    HubKind Hub { get; }
    Task SendGroupRemoveSubscriptionsAsync(string defaultGroupName, string[] ids, CancellationToken cancellationToken);
    Task SystemGroupPublish(string groupId, IEnumerable<IGlobalMessageModel> models, CancellationToken cancellationToken);
    Task SendGroupOnHeartbeatAsync(string defaultGroupName, CancellationToken cancellationToken);
}

public interface IMessagePublisher<T> : IMessagePublisher
{
    Task GroupPublish(string defaultGroupName, T[] models, CancellationToken cancellationToken);
    Task GroupPublishEmptyArray(string defaultGroupName, CancellationToken cancellationToken);
}
