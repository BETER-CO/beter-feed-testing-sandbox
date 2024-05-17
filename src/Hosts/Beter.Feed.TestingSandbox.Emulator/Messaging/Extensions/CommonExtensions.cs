using System.Text;

namespace Beter.Feed.TestingSandbox.Emulator.Messaging.Extensions;

public static class CommonExtensions
{
    public static string ToUtf8String(this byte[] source)
    {
        return source == null ? null : Encoding.UTF8.GetString(source);
    }
}
