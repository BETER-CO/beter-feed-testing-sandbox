namespace Beter.TestingTool.Generator.Application.Services.Playbacks.Transformations;

public interface IMatchIdGenerator
{
    string Generate(int testCaseId, int runCount);

    Dictionary<string, string> Generate(IEnumerable<string> matchIds, int testCaseId, int runCount);
}

public class MatchIdGenerator : IMatchIdGenerator
{
    public string Generate(int testCaseId, int runCount)
    {
        var emulatedTestCaseIdHex = testCaseId.ToString("X8");
        var dateTimeHexValue = DateTime.UtcNow.Ticks.ToString("X");

        return $"{emulatedTestCaseIdHex}-{dateTimeHexValue}-{runCount:X8}";
    }

    public Dictionary<string, string> Generate(IEnumerable<string> matchIds, int testCaseId, int runCount)
    {
        return matchIds.ToDictionary(
           matchId => matchId,
           matchId => Generate(testCaseId, runCount));
    }
}
