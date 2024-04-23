namespace Beter.TestingTool.Generator.Domain;

public sealed class SteeringCommand
{
    public SteeringCommandType CommandType;
}

public enum SteeringCommandType
{
    StartHeartbeat = 1,
    StopHeartbeat = 2
}
