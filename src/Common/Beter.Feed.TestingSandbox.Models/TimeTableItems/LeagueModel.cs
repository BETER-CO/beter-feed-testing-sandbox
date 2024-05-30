using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.TimeTableItems;

[MessagePackObject]
public class LeagueModel : NamedIdentityModelBase
{
    public LeagueModel()
    {

    }
    [Key("competitorType")] public int CompetitorType { get; set; }
    [Key("brand")] public BrandModel Brand { get; set; }
    [Key("tier")] public int Tier { get; set; }
}
