#nullable enable

using System.Globalization;
using System.Text.Json;

namespace Beter.Feed.TestingSandbox.Server.Cases;

/// <summary>
/// The case set of the sandbox: the case files of the image, the ids <c>retired.json</c>
/// lists, and the cases loaded at runtime. A runtime case lives in memory only.
/// </summary>
public sealed class CaseCatalogue
{
    /// <summary>The first id of the range reserved for cases loaded at runtime.</summary>
    public const int FirstRuntimeId = 100000;

    /// <summary>The file that lists the retired ids.</summary>
    public const string RetiredFileName = "retired.json";

    private readonly CaseSchema _schema;
    private readonly object _gate = new();
    private readonly SortedDictionary<int, CaseEntry> _cases = new();
    private readonly TaskCompletionSource _loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _nextRuntimeId = FirstRuntimeId;

    /// <summary>Initializes a new instance of the <see cref="CaseCatalogue"/> class.</summary>
    /// <param name="schema">The case schema.</param>
    public CaseCatalogue(CaseSchema schema)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
    }

    /// <summary>Gets a value indicating whether the case set is loaded.</summary>
    public bool IsLoaded => _loaded.Task.IsCompletedSuccessfully;

    /// <summary>Gets the task that completes when the case set is loaded, or fails with the load.</summary>
    public Task Loaded => _loaded.Task;

    /// <summary>Loads the case files and <c>retired.json</c> of a directory.</summary>
    /// <param name="directory">The directory of the case set.</param>
    /// <exception cref="InvalidDataException">A file of the directory is not a valid case file or retired list.</exception>
    public void Load(string directory)
    {
        try
        {
            var loaded = ReadCaseSet(directory);
            lock (_gate)
            {
                foreach (var entry in loaded)
                {
                    _cases[entry.CaseId] = entry;
                }
            }

            _loaded.TrySetResult();
        }
        catch (Exception exception)
        {
            _loaded.TrySetException(exception);
            throw;
        }
    }

    /// <summary>Lists the catalogue, ordered by id.</summary>
    /// <param name="tags">The tags a case must carry, all of them.</param>
    /// <param name="active">Whether to list the active cases.</param>
    /// <param name="retired">Whether to list the retired ids.</param>
    /// <returns>The matching entries.</returns>
    public IReadOnlyList<CaseEntry> List(IReadOnlyCollection<string> tags, bool active, bool retired)
    {
        ArgumentNullException.ThrowIfNull(tags);
        lock (_gate)
        {
            return _cases.Values
                .Where(entry => entry.Retired ? retired : active)
                .Where(entry => tags.All(tag => entry.Summary?.Tags.Contains(tag, StringComparer.Ordinal) == true))
                .ToArray();
        }
    }

    /// <summary>Finds one id of the catalogue.</summary>
    /// <param name="caseId">The case id.</param>
    /// <returns>The entry, or null for an unknown id.</returns>
    public CaseEntry? Find(int caseId)
    {
        lock (_gate)
        {
            return _cases.GetValueOrDefault(caseId);
        }
    }

    /// <summary>Loads a case posted at runtime. Nothing is written to disk.</summary>
    /// <param name="runtimeCase">The posted case.</param>
    /// <returns>The loaded case, or the reasons it was refused.</returns>
    public RuntimeCaseResult AddRuntimeCase(JsonElement runtimeCase)
    {
        var errors = _schema.ValidateRuntimeCase(runtimeCase);
        if (errors.Count > 0)
        {
            return new RuntimeCaseResult(null, errors);
        }

        lock (_gate)
        {
            int caseId;
            if (runtimeCase.TryGetProperty("caseId", out var posted))
            {
                if (!posted.TryGetInt32(out caseId))
                {
                    var notAnId = new SchemaError("/caseId", CaseSchema.CaseIdKeyword, "The case id is not a whole number of the id range.");
                    return new RuntimeCaseResult(null, new[] { notAnId });
                }

                if (_cases.ContainsKey(caseId))
                {
                    var inUse = new SchemaError("/caseId", CaseSchema.CaseIdKeyword, $"Case id {caseId} is in use.");
                    return new RuntimeCaseResult(null, new[] { inUse });
                }
            }
            else
            {
                while (_cases.ContainsKey(_nextRuntimeId))
                {
                    _nextRuntimeId++;
                }

                caseId = _nextRuntimeId++;
            }

            var entry = new CaseEntry
            {
                CaseId = caseId,
                Summary = Summarize(runtimeCase),
                Content = WithCaseId(runtimeCase, caseId),
            };
            _cases.Add(caseId, entry);
            return new RuntimeCaseResult(entry, Array.Empty<SchemaError>());
        }
    }

    private static Dictionary<int, int?> ReadRetired(string path)
    {
        var retired = new Dictionary<int, int?>();
        if (!File.Exists(path))
        {
            return retired;
        }

        using var document = Parse(path);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException($"'{RetiredFileName}' is not a list.");
        }

        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("caseId", out var caseId) || !caseId.TryGetInt32(out var id) || id < 1
                || !item.TryGetProperty("date", out var date) || date.ValueKind != JsonValueKind.String
                || !DateOnly.TryParseExact(date.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                || !item.TryGetProperty("replacedBy", out var replacedBy)
                || !(replacedBy.ValueKind == JsonValueKind.Null || (replacedBy.TryGetInt32(out var replacement) && replacement >= 1)))
            {
                throw new InvalidDataException(
                    $"'{RetiredFileName}': an entry carries caseId, date (yyyy-MM-dd) and replacedBy (an id or null); found {item.GetRawText()}.");
            }

            if (!retired.TryAdd(id, replacedBy.ValueKind == JsonValueKind.Null ? null : replacedBy.GetInt32()))
            {
                throw new InvalidDataException($"'{RetiredFileName}' lists the id {id} twice.");
            }
        }

        return retired;
    }

    private static JsonDocument Parse(string path)
    {
        try
        {
            return JsonDocument.Parse(File.ReadAllBytes(path));
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"'{Path.GetFileName(path)}' is not JSON: {exception.Message}", exception);
        }
    }

    private static CaseSummary Summarize(JsonElement caseFile) => new(
        caseFile.GetProperty("version").GetString()!,
        caseFile.GetProperty("class").GetString()!,
        caseFile.TryGetProperty("sport", out var sport) ? WholeNumber(sport) : null,
        caseFile.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()!).ToArray(),
        caseFile.GetProperty("description").GetString()!,
        caseFile.GetProperty("terminal").GetString()!,
        caseFile.TryGetProperty("covers", out var covers) ? covers.EnumerateArray().Select(SectionOf).ToArray() : [],
        caseFile.TryGetProperty("duration", out var duration) ? WholeNumber(duration) : SpanOf(caseFile.GetProperty("script")))
    {
        ContractFetchedAt = caseFile.TryGetProperty("contract", out var contract) ? contract.GetProperty("fetchedAt").GetString() : null,
        ContractNewestChange = caseFile.TryGetProperty("contract", out var pin) ? pin.GetProperty("newestChange").GetString() : null,
        MaxAcceleration = caseFile.TryGetProperty("maxAcceleration", out var maxAcceleration)
            ? maxAcceleration.GetDouble()
            : CaseSummary.DefaultMaxAcceleration,
        Start = caseFile.TryGetProperty("start", out var start) ? start.GetString() : null,
        Expected = caseFile.TryGetProperty("expected", out var expected)
            ? expected.EnumerateArray().Select(entry => entry.GetProperty("id").GetString()!).ToArray()
            : [],
    };

    private static DocumentationSection SectionOf(JsonElement section) =>
        new(section.GetProperty("page").GetString()!, section.GetProperty("section").GetString()!);

    /// <summary>
    /// A number the case schema accepts as an integer: JSON Schema takes <c>6.0</c> and a
    /// number beyond 64 bits for one.
    /// </summary>
    private static long WholeNumber(JsonElement number) =>
        number.TryGetInt64(out var value) ? value : (long)Math.Clamp(number.GetDouble(), long.MinValue, long.MaxValue);

    /// <summary>The feed time a script spans, in whole seconds: its last offset.</summary>
    private static long SpanOf(JsonElement script) =>
        script.EnumerateArray()
            .Select(entry => entry.TryGetProperty("at", out var at) ? WholeNumber(at) : 0)
            .DefaultIfEmpty(0)
            .Max() / 1000;

    private static byte[] WithCaseId(JsonElement runtimeCase, int caseId)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("caseId", caseId);
            foreach (var property in runtimeCase.EnumerateObject().Where(property => !property.NameEquals("caseId")))
            {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        return buffer.ToArray();
    }

    private List<CaseEntry> ReadCaseSet(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The case directory '{directory}' does not exist.");
        }

        var retired = ReadRetired(Path.Combine(directory, RetiredFileName));
        var entries = new Dictionary<int, CaseEntry>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.json").Order(StringComparer.Ordinal))
        {
            var name = Path.GetFileName(path);
            if (string.Equals(name, RetiredFileName, StringComparison.Ordinal))
            {
                continue;
            }

            using var document = Parse(path);
            var errors = _schema.ValidateCaseFile(document.RootElement);
            if (errors.Count > 0)
            {
                var first = errors[0];
                throw new InvalidDataException(
                    $"'{name}' fails the case schema at '{first.InstanceLocation}' ({first.KeywordLocation}): {first.Error}");
            }

            if (!document.RootElement.GetProperty("caseId").TryGetInt32(out var caseId))
            {
                throw new InvalidDataException($"'{name}': the case id is not a whole number of the id range.");
            }

            if (!string.Equals(name, string.Create(CultureInfo.InvariantCulture, $"{caseId}.json"), StringComparison.Ordinal))
            {
                throw new InvalidDataException($"'{name}' carries the case id {caseId}; a case file is named by its id.");
            }

            if (caseId >= FirstRuntimeId)
            {
                throw new InvalidDataException($"'{name}': ids from {FirstRuntimeId} up are reserved for runtime cases.");
            }

            var isRetired = retired.Remove(caseId, out var replacedBy);
            entries.Add(caseId, new CaseEntry
            {
                CaseId = caseId,
                Retired = isRetired,
                ReplacedBy = replacedBy,
                Summary = Summarize(document.RootElement),
                FilePath = path,
            });
        }

        foreach (var (caseId, replacedBy) in retired)
        {
            entries.Add(caseId, new CaseEntry { CaseId = caseId, Retired = true, ReplacedBy = replacedBy });
        }

        return entries.Values.ToList();
    }
}
