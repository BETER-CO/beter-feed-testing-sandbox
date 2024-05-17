using Beter.Feed.TestingSandbox.Generator.Application.Contracts;

namespace Beter.Feed.TestingSandbox.Generator.Application.Services;

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
