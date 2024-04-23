using MessagePack;

namespace Beter.TestingTools.Models.Scoreboards;

[MessagePackObject]
public class TimerModel
{
    public TimerModel()
    {

    }
    [Key("timeStamp")] public long TimeStamp { get; set; }
    [Key("timerValue")] public long TimerValue { get; set; }
    [Key("accelerationFactor")] public double AccelerationFactor { get; set; }
    [Key("enabled")] public bool Enabled { get; set; }
    [Key("direction")] public int Direction { get; set; }
}
