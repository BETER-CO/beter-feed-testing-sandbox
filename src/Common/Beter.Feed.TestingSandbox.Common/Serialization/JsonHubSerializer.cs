using Beter.Feed.TestingSandbox.Common.Serialization.Converters;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Beter.Feed.TestingSandbox.Common.Serialization
{
    /// <summary>
    /// JSON serializer similar to FeedHub's serialization format.
    /// </summary>
    public static class JsonHubSerializer
    {
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = false,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            MaxDepth = 64,
            DictionaryKeyPolicy = null,
            DefaultBufferSize = 16 * 1024,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new AdditionInfoConverter() }
        };

        public static string Serialize<TValue>(TValue value) => JsonSerializer.Serialize(value, _serializerOptions);
        public static TValue Deserialize<TValue>(string json) => JsonSerializer.Deserialize<TValue>(json, _serializerOptions);
        public static TValue Deserialize<TValue>(JsonNode json) => json.Deserialize<TValue>(_serializerOptions);
        public static object Deserialize(string json, Type type) => JsonSerializer.Deserialize(json, type, _serializerOptions);
    }
}
