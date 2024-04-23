﻿using MessagePack;

namespace Beter.TestingTools.Models.TimeTableItems;

[MessagePackObject]
public class LeagueModel : NamedIdentityModelBase
{
    public LeagueModel()
    {

    }
    [Key("competitorType")] public int CompetitorType { get; set; }
    [Key("brand")] public BrandModel Brand { get; set; }
}