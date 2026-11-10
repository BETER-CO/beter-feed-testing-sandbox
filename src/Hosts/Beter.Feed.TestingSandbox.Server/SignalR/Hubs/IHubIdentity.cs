using Beter.Feed.TestingSandbox.Common.Enums;

namespace Beter.Feed.TestingSandbox.Server.SignalR.Hubs;

public interface IHubIdentity
{
    HubKind Hub { get; }
    HubVersion Version { get; }
}
