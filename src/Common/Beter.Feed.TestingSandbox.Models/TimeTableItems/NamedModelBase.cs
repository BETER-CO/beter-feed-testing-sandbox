using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.TimeTableItems;

[MessagePackObject]
public class NamedModelBase
{
    public NamedModelBase()
    {

    }
    [Key("name")] public Dictionary<string, string> Name { get; set; }
}


