using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.Scoreboards;

[MessagePackObject]
public class RegulationsModel
{
    public RegulationsModel()
    {

    }
    [Key("matchFormat")] public int? MatchFormat { get; set; }
    [Key("canBeDrawInMap")] public bool? CanBeDrawInMap { get; set; }
    [Key("maps")] public IEnumerable<string> Maps { get; set; }
    [Key("realTimeDuration")] public int? RealTimeDuration { get; set; }
    [Key("extraPeriodRealTimeDuration")] public int? ExtraPeriodRealTimeDuration { get; set; }
    [Key("roundQty")] public int? RoundQty { get; set; }
}