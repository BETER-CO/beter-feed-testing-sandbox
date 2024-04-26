using System.Text.Json.Serialization;

namespace Beter.TestingTools.Generator.Domain;

public sealed class SteeringCommand
{
    public SteeringCommandType CommandType { get; set; }
}

public enum SteeringCommandType
{
    StartHeartbeat = 1,
    StopHeartbeat = 2
}
