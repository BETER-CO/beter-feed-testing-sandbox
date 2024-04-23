namespace Beter.TestingTools.Models;

public interface IFeedMessage : IIdentityModel
{
    public int MsgType { get; set; }
    public int? SportId { get; set; }
    public long Offset { get; set; }
}
