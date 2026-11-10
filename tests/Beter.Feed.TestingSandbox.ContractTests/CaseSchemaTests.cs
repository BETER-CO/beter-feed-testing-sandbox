using System.Text.Json.Nodes;
using LateApexEarlySpeed.Json.Schema;

namespace Beter.Feed.TestingSandbox.ContractTests;

public class CaseSchemaTests
{
    private const string ReshapedCase1001 = "schemas/case/examples/1001.excerpt.json";
    private const string FaultDirectivesCase = "schemas/case/examples/fault-directives.json";

    private static readonly JsonValidator CaseSchema = ContractFiles.ValidatorOf(ContractFiles.CaseSchema);

    [Fact]
    public void Reshaped_case_1001_validates_against_the_case_schema()
    {
        var reshaped = ContractFiles.Load(ReshapedCase1001);

        Assert.Null(CaseSchema.Failure(reshaped));
        Assert.Equal(1001, reshaped["caseId"]!.GetValue<int>());
        Assert.Equal(
            new[] { "incident", "scoreboard", "time_table", "trading" },
            reshaped["script"]!.AsArray().Select(entry => entry!["channel"]!.GetValue<string>()).Distinct().Order());
    }

    [Fact]
    public void Case_with_the_three_fault_directives_validates()
    {
        var sample = ContractFiles.Load(FaultDirectivesCase);
        var directives = sample["script"]!.AsArray().Where(entry => entry!["directive"] is not null).ToArray();

        Assert.Null(CaseSchema.Failure(sample));
        Assert.Equal(
            new[] { "connectionDrop", "heartbeatStop", "heartbeatResume" },
            directives.Select(entry => entry!["directive"]!.GetValue<string>()));
        Assert.Contains(directives, entry => entry!["after"]?["event"]?.GetValue<string>() == "snapshotServed");
    }

    [Fact]
    public void Case_with_an_unknown_directive_is_rejected()
    {
        var sample = ContractFiles.Load(FaultDirectivesCase);
        Directive(sample, "heartbeatStop")["directive"] = "clientDisconnect";

        Assert.NotNull(CaseSchema.Failure(sample));
    }

    [Fact]
    public void Directive_with_both_at_and_after_is_rejected()
    {
        var sample = ContractFiles.Load(FaultDirectivesCase);
        Directive(sample, "heartbeatStop")["after"] = new JsonObject { ["event"] = "snapshotServed", ["batch"] = 1 };

        Assert.NotNull(CaseSchema.Failure(sample));
    }

    [Fact]
    public void Directive_without_channels_is_rejected()
    {
        var sample = ContractFiles.Load(FaultDirectivesCase);
        Directive(sample, "connectionDrop").Remove("channels");

        Assert.NotNull(CaseSchema.Failure(sample));
    }

    [Fact]
    public void Case_with_an_unknown_field_is_rejected()
    {
        var sample = ContractFiles.Load(FaultDirectivesCase);
        sample["messages"] = new JsonArray();

        Assert.NotNull(CaseSchema.Failure(sample));
    }

    [Fact]
    public void Case_file_without_its_id_is_rejected()
    {
        var sample = ContractFiles.Load(FaultDirectivesCase);
        sample.AsObject().Remove("caseId");

        Assert.NotNull(CaseSchema.Failure(sample));
    }

    private static JsonObject Directive(JsonNode sample, string directive) =>
        sample["script"]!.AsArray()
            .Single(entry => entry!["directive"]?.GetValue<string>() == directive)!
            .AsObject();
}
