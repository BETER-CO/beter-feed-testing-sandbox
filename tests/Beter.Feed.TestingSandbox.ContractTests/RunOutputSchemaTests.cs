using System.Text.Json.Nodes;
using LateApexEarlySpeed.Json.Schema;

namespace Beter.Feed.TestingSandbox.ContractTests;

public class RunOutputSchemaTests
{
    private const string Examples = "schemas/run-output/v1/examples/";

    private static readonly JsonValidator RunSchema = ContractFiles.ValidatorOf(ContractFiles.RunSchema);
    private static readonly JsonValidator MessageSchema = ContractFiles.ValidatorOf(ContractFiles.MessageSchema);
    private static readonly JsonValidator EventSchema = ContractFiles.ValidatorOf(ContractFiles.EventSchema);

    [Fact]
    public void Sample_run_manifest_validates()
    {
        Assert.Null(RunSchema.Failure(ContractFiles.Load(Examples + "run.json")));
    }

    [Fact]
    public void Sample_message_line_validates()
    {
        var lines = Lines("messages.jsonl");

        Assert.Single(lines);
        Assert.All(lines, line => Assert.Null(MessageSchema.Failure(line)));
    }

    [Fact]
    public void Sample_event_lines_validate()
    {
        var lines = Lines("events.jsonl");

        Assert.Equal(7, lines.Length);
        Assert.All(lines, line => Assert.Null(EventSchema.Failure(line)));
    }

    [Fact]
    public void Run_manifest_with_an_unknown_status_is_rejected()
    {
        var manifest = ContractFiles.Load(Examples + "run.json");
        manifest["status"] = "passed";

        Assert.NotNull(RunSchema.Failure(manifest));
    }

    [Fact]
    public void Message_line_without_its_payload_is_rejected()
    {
        var line = Lines("messages.jsonl")[0];
        line.AsObject().Remove("payload");

        Assert.NotNull(MessageSchema.Failure(line));
    }

    [Fact]
    public void Connected_event_without_its_connection_is_rejected()
    {
        var connected = Lines("events.jsonl").First(line => line["kind"]!.GetValue<string>() == "connected");
        connected.AsObject().Remove("connectionId");

        Assert.NotNull(EventSchema.Failure(connected));
    }

    private static JsonNode[] Lines(string name) =>
        ContractFiles.ReadText(Examples + name)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => JsonNode.Parse(line)!)
            .ToArray();
}
