namespace Beter.TestingTools.Generator.Contracts.Requests;

public record SetHeartbeatCommandRequest
{
    public int Command { get; init; }
}