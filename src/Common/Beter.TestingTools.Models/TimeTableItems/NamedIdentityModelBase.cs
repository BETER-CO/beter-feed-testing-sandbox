using MessagePack;

namespace Beter.TestingTools.Models.TimeTableItems;

[MessagePackObject]
public class NamedIdentityModelBase : NamedModelBase, IIdentityModel
{
    public NamedIdentityModelBase()
    {

    }

    [Key("id")] public string Id { get; set; }
}


