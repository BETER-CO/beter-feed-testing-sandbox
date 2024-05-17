using Beter.Feed.TestingSandbox.Hosting;

namespace Beter.Feed.TestingSandbox.Consumer;

public class Program
{
    public static void Main(string[] args)
    {
        HostStarter.Start<Startup>(args, "feed-testing-sandbox", "consumer");
    }
}


