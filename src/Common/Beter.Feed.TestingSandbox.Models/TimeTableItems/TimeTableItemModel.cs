using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.TimeTableItems;

[MessagePackObject]
public class TimeTableItemModel : NamedIdentityModelBase, IFeedMessage
{
    public TimeTableItemModel()
    {

    }
    [Key("sport")] public string Sport { get; set; }
    [Key("sportId")] public int? SportId { get; set; }
    [Key("startDate")] public DateTime? StartDate { get; set; }
    [Key("status")] public int? Status { get; set; }
    [Key("country")] public CountryModel Country { get; set; }
    [Key("category")] public CategoryModel Category { get; set; }
    [Key("league")] public LeagueModel League { get; set; }
    [Key("tournament")] public TournamentModel Tournament { get; set; }
    [Key("venue")] public VenueModel Venue { get; set; }
    [Key("participants")] public ParticipantModel[] Participants { get; set; }
    [Key("broadcastUrl")] public string BroadcastUrl { get; set; }
    [Key("deleted")] public DateTime? Deleted { get; set; }
    [Key("timestamp")] public long Timestamp { get; set; }
    [Key("willBeLive")] public bool WillBeLive { get; set; }
    [Key("willBePrematch")] public bool WillBePrematch { get; set; }
    [Key("messageType")] public int MsgType { get; set; }
    [Key("offset")] public long Offset { get; set; }
    [Key("extra")] public Dictionary<string, string> Extra { get; set; }
}
