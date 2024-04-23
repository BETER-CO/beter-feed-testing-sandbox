using Beter.TestingTools.Common.Enums;

namespace Beter.TestingTools.Emulator.SignalR.Hubs;

public interface IHubIdentity
{
    HubKind Hub { get; }
    HubVersion Version { get; }
}
