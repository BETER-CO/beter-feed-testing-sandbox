namespace Beter.TestingTool.Generator.Contracts.Requests;

public record SetHeartbeatCommandRequest
{
    public int Command { get; init; }
}