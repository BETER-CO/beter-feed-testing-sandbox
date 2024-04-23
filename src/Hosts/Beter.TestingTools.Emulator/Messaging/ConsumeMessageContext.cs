namespace Beter.TestingTools.Emulator.Messaging;

public sealed class ConsumeMessageContext
{
    public object MessageObject { get; set; }

    public Type MessageType { get; set; }

    public Dictionary<string, string> MessageHeaders { get; set; } = new Dictionary<string, string>();
}