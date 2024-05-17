using Beter.Feed.TestingSandbox.Models;
using MessagePack;

namespace Beter.Feed.TestingSandbox.Models.TimeTableItems;

[MessagePackObject]
public class NamedIdentityModelBase : NamedModelBase, IIdentityModel
{
    public NamedIdentityModelBase()
    {

    }

    [Key("id")] public string Id { get; set; }
}


