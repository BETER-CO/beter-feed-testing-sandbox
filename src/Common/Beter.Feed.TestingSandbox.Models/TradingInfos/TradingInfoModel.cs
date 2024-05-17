using Beter.Feed.TestingSandbox.Models;
using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.TradingInfos;

[MessagePackObject]
public class TradingInfoModel : IFeedMessage
{
    public TradingInfoModel()
    {

    }
    [Key("id")] public string Id { get; set; }
    [Key("sportId")] public int? SportId { get; set; }
    [Key("tradingStatus")] public int? TradingStatus { get; set; }
    [Key("lineType")] public int? LineType { get; set; }
    [Key("markets")] public IEnumerable<MarketModel> Markets { get; set; }
    [Key("timestamp")] public long Timestamp { get; set; }
    [Key("willBeLive")] public bool? WillBeLive { get; set; }
    [Key("willBePrematch")] public bool? WillBePrematch { get; set; }
    [Key("messageType")] public int MsgType { get; set; }
    [Key("offset")] public long Offset { get; set; }
}
