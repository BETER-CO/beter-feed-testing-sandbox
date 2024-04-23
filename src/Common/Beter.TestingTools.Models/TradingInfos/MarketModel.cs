using MessagePack;

namespace Beter.TestingTools.Models.TradingInfos;

[MessagePackObject]
public class MarketModel
{
    public MarketModel()
    {

    }
    [Key("id")] public string Id { get; set; }
    [Key("interval")] public int? Interval { get; set; }
    [Key("resultType")] public int? ResultType { get; set; }
    [Key("marketType")] public int? MarketType { get; set; }
    [Key("marketValue")] public string MarketValue { get; set; }
    [Key("subinterval")] public int? Subinterval { get; set; }
    [Key("outcomes")] public IEnumerable<OutcomeModel> Outcomes { get; set; }
}
