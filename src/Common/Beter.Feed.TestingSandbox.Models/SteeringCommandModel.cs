namespace Beter.Feed.TestingSandbox.Models
{
    public sealed class SteeringCommandModel
    {
        public SteeringCommandType CommandType { get; set; }

    }

    public enum SteeringCommandType
    {
        StartHeartbeat = 1,
        StopHeartbeat = 2
    }
}
