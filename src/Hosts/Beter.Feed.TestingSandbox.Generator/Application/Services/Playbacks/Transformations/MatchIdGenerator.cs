namespace Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;

public interface IMatchIdGenerator
{
    string Generate(int testCaseId, int runCount);
}

public class MatchIdGenerator : IMatchIdGenerator
{
    public string Generate(int testCaseId, int runCount)
    {
        var formattedTestCaseId = testCaseId.ToString("D8");
        var formattedRunCount = runCount.ToString("D8");
        var dateTimeHexValue = DateTime.UtcNow.Ticks.ToString("X");

        return $"{formattedTestCaseId}-{dateTimeHexValue}-{formattedRunCount}";
    }
}
