using Beter.TestingTool.Generator.Application.Contracts;

namespace Beter.TestingTool.Generator.Application.Services;

public class SequenceNumberProvider : ISequenceNumberProvider
{
    private int counter = 0;
    private readonly object counterLock = new object();

    public int GetNext()
    {
        lock (counterLock)
        {
            return ++counter;
        }
    }
}
