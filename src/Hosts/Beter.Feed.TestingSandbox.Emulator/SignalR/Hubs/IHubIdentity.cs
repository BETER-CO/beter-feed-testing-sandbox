using Beter.Feed.TestingSandbox.Common.Enums;

namespace Beter.Feed.TestingSandbox.Emulator.SignalR.Hubs;

public interface IHubIdentity
{
    HubKind Hub { get; }
    HubVersion Version { get; }
}
