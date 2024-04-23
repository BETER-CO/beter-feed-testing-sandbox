using MessagePack;

namespace Beter.TestingTools.Models.TimeTableItems;

[MessagePackObject]
public class VenueModel : NamedIdentityModelBase
{
    public VenueModel()
    {

    }
    [Key("console")] public string Console { get; set; }
}
