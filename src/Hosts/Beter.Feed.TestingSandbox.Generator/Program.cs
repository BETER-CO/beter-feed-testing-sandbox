using Beter.Feed.TestingSandbox.Hosting;

namespace Beter.Feed.TestingSandbox.Generator;

public class Program
{
    public static void Main(string[] args)
    {
        HostStarter.Start<Startup>(args, "feed-testing-sandbox", "generator");
    }
}

