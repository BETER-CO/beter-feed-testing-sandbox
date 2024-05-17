using Beter.Feed.TestingSandbox.Common.Enums;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services.TestScenarios.MessageHandlers.Offsets
{
    public interface IOffsetStorage
    {
        int GetOffsetForUpdateMessage(string matchId, HubKind hubKind);
        int GetOffsetForNonUpdateMessage(string matchId, HubKind hubKind);
    }
}
