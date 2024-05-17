using System.Text.Json.Serialization;

namespace Beter.Feed.TestingSandbox.Generator.Domain;

public sealed class SteeringCommand
{
    public SteeringCommandType CommandType { get; set; }
}

public enum SteeringCommandType
{
    StartHeartbeat = 1,
    StopHeartbeat = 2
}
