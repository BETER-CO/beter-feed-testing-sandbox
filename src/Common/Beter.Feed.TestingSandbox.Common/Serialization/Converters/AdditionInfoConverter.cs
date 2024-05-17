using Beter.Feed.TestingSandbox.Common.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beter.Feed.TestingSandbox.Common.Serialization.Converters
{
    public class AdditionInfoConverter : JsonConverter<AdditionalInfo>
    {
        public override AdditionalInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var document = JsonDocument.ParseValue(ref reader))
            {
                var dictionary = new Dictionary<string, object>();

                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    dictionary.Add(property.Name, GetValue(property.Value));
                }

                return new AdditionalInfo(dictionary);
            }
        }

        public override void Write(Utf8JsonWriter writer, AdditionalInfo value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.GetInfo(), options);
        }

        private object GetValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    {
                        var obj = new Dictionary<string, object>();
                        foreach (JsonProperty property in element.EnumerateObject())
                        {
                            obj[property.Name] = GetValue(property.Value);
                        }

                        return obj;
                    }
                case JsonValueKind.Array:
                    {
                        var list = new List<object>();
                        foreach (JsonElement item in element.EnumerateArray())
                        {
                            list.Add(GetValue(item));
                        }
                        return list;
                    }
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    return element.GetInt32();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    throw new NotSupportedException($"Unsupported JSON value kind: {element.ValueKind}");
            }
        }
    }
}
