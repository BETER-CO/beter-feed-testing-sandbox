namespace Beter.Feed.TestingSandbox.Models;

public interface IFeedMessage : IIdentityModel
{
    public int MsgType { get; set; }
    public int? SportId { get; set; }
    public long Offset { get; set; }
}
