using Beter.TestingTools.Common.Constants;
using Beter.TestingTools.Models;
using System.Text.Json.Nodes;

namespace Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations.Helpers;

public record FeedMessageWrapper : IFeedMessage, IIdentityModel
{
    private readonly JsonNode _message;

    public FeedMessageWrapper(JsonNode message)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public string Id
    {
        get => _message[MessageProperties.Id].GetValue<string>();
        set => _message[MessageProperties.Id] = value;
    }

    public int MsgType
    {
        get => _message[MessageProperties.MsgType].GetValue<int>();
        set => _message[MessageProperties.MsgType] = value;
    }

    public long Offset
    {
        get => _message[MessageProperties.Offset].GetValue<long>();
        set => _message[MessageProperties.Offset] = value;
    }

    public int? SportId
    {
        get => _message[MessageProperties.SportId].GetValue<int?>();
        set => _message[MessageProperties.SportId] = value;
    }
}
