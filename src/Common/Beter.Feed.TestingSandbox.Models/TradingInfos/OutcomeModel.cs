using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.TradingInfos;

[MessagePackObject]
public class OutcomeModel
{
    public OutcomeModel()
    {

    }
    [Key("id")] public string Id { get; set; }
    [Key("outcomeType")] public int? OutcomeType { get; set; }
    [Key("outcomeValue")] public string OutcomeValue { get; set; }
    [Key("price")] public decimal Price { get; set; }
    [Key("status")] public int? Status { get; set; }
    [Key("outcomeResult")] public int? OutcomeResult { get; set; }
    [Key("prices")] public Dictionary<string, object> Prices { get; set; }
}
