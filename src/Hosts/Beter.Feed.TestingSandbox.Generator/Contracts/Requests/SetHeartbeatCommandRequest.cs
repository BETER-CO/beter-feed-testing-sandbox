namespace Beter.Feed.TestingSandbox.Generator.Contracts.Requests;

public record SetHeartbeatCommandRequest
{
    public int Command { get; init; }
}