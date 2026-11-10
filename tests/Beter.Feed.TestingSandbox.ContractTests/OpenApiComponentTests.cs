using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.ContractTests;

public class OpenApiComponentTests
{
    private const string ReshapedCase1001 = "schemas/case/examples/1001.excerpt.json";
    private const string FaultDirectivesCase = "schemas/case/examples/fault-directives.json";

    [Fact]
    public void Runtime_case_takes_an_id_from_100000_up_or_none()
    {
        var runtimeCase = ContractFiles.ComponentValidatorOf("RuntimeCase");
        var withRuntimeId = ContractFiles.Load(FaultDirectivesCase);
        var withoutId = ContractFiles.Load(FaultDirectivesCase);
        withoutId.AsObject().Remove("caseId");
        var withUnknownField = ContractFiles.Load(FaultDirectivesCase);
        withUnknownField["messages"] = new JsonArray();

        Assert.Equal(100000, withRuntimeId["caseId"]!.GetValue<int>());
        Assert.Null(runtimeCase.Failure(withRuntimeId));
        Assert.Null(runtimeCase.Failure(withoutId));
        Assert.NotNull(runtimeCase.Failure(ContractFiles.Load(ReshapedCase1001)));
        Assert.NotNull(runtimeCase.Failure(withUnknownField));
    }

    [Fact]
    public void Run_request_needs_a_case_and_an_apiKey()
    {
        var runRequest = ContractFiles.ComponentValidatorOf("RunRequest");

        Assert.Null(runRequest.Failure(Json("""{"caseId": 1003, "apiKey": "ci-run-42"}""")));
        Assert.Null(runRequest.Failure(Json(
            """{"caseId": 1003, "apiKey": "ci-run-42", "acceleration": 10, "start": "now", "waitTimeoutSeconds": 30}""")));
        Assert.NotNull(runRequest.Failure(Json("""{"caseId": 1003}""")));
        Assert.NotNull(runRequest.Failure(Json("""{"caseId": 1003, "apiKey": "ci-run-42", "acceleration": 0.1}""")));
        Assert.NotNull(runRequest.Failure(Json("""{"caseId": 1003, "apiKey": "ci-run-42", "start": "later"}""")));
        Assert.NotNull(runRequest.Failure(Json("""{"caseId": 1003, "apiKey": "ci-run-42", "waitTimeoutSeconds": 0}""")));
        Assert.NotNull(runRequest.Failure(Json("""{"caseId": 1003, "apiKey": "ci-run-42", "timeOffsetInMinutes": 5}""")));
    }

    [Fact]
    public void Case_detail_is_the_case_file_with_its_status()
    {
        var caseDetail = ContractFiles.ComponentValidatorOf("CaseDetail");
        var served = ContractFiles.Load(ReshapedCase1001);
        served["status"] = "retired";
        served["replacedBy"] = 1024;

        Assert.Null(caseDetail.Failure(served));
        Assert.NotNull(caseDetail.Failure(ContractFiles.Load(ReshapedCase1001)));
    }

    [Fact]
    public void Catalogue_row_carries_the_case_fields_and_the_duration()
    {
        var catalogueRow = ContractFiles.ComponentValidatorOf("CatalogueRow");
        var row = Json(
            """
            {
              "caseId": 1001, "version": "1.1.0", "status": "active", "class": "baseline", "sport": 6,
              "tags": ["smoke", "baseline", "efootball"], "description": "Regular_FIFA_Match", "terminal": "Finished",
              "covers": { "count": 0, "sections": [] }, "duration": 1681
            }
            """);
        var withUnknownClass = row.DeepClone();
        withUnknownClass["class"] = "regular";
        var withoutDuration = row.DeepClone();
        withoutDuration.AsObject().Remove("duration");
        var withoutSport = row.DeepClone();
        withoutSport.AsObject().Remove("sport");

        Assert.Null(catalogueRow.Failure(row));
        Assert.Null(catalogueRow.Failure(withoutSport));
        Assert.NotNull(catalogueRow.Failure(withUnknownClass));
        Assert.NotNull(catalogueRow.Failure(withoutDuration));
    }

    [Fact]
    public void Version_answer_carries_the_contract_pin_or_null()
    {
        var version = ContractFiles.ComponentValidatorOf("Version");

        Assert.Null(version.Failure(Json(
            """
            {
              "sandboxVersion": "3.0.0", "outputVersion": 1,
              "contract": { "fetchedAt": "2026-10-31", "newestChange": "2026-10-30 Added new sport Mobile Legends BB" }
            }
            """)));
        Assert.Null(version.Failure(Json("""{"sandboxVersion": "3.0.0", "outputVersion": 1, "contract": null}""")));
        Assert.NotNull(version.Failure(Json("""{"sandboxVersion": "3.0.0", "outputVersion": 1, "contract": {}}""")));
    }

    [Fact]
    public void Retired_case_names_its_replacement_or_null()
    {
        var catalogueRow = ContractFiles.ComponentValidatorOf("CatalogueRow");
        var retiredError = ContractFiles.ComponentValidatorOf("RetiredError");
        var retired = Json(
            """
            {
              "caseId": 1014, "version": "1.0.0", "status": "retired", "class": "baseline", "sport": 6,
              "tags": [], "description": "Retired", "terminal": "Finished",
              "covers": { "count": 0, "sections": [] }, "duration": 0
            }
            """);
        var replaced = retired.DeepClone();
        replaced["replacedBy"] = 1024;
        var withoutReplacement = retired.DeepClone();
        withoutReplacement["replacedBy"] = null;

        Assert.NotNull(catalogueRow.Failure(retired));
        Assert.Null(catalogueRow.Failure(replaced));
        Assert.Null(catalogueRow.Failure(withoutReplacement));
        Assert.Null(retiredError.Failure(Json("""{"error": "gone", "detail": "Case 1014 is retired.", "replacedBy": null}""")));
        Assert.Null(retiredError.Failure(Json("""{"error": "gone", "detail": "Case 1003 is retired.", "replacedBy": 1024}""")));
        Assert.NotNull(retiredError.Failure(Json("""{"error": "gone", "detail": "Case 1014 is retired."}""")));
    }

    private static JsonNode Json(string text) => JsonNode.Parse(text)!;
}
