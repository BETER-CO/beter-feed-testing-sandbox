using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beter.Feed.TestingSandbox.Common.Extensions;

public static class MessageExtensions
{
    public static void SetValue<TValue>(this JsonNode jsonNode, string key, TValue value)
    {
        if (jsonNode is JsonObject jsonObject)
        {
            foreach (var property in jsonObject)
            {
                if (string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    jsonObject[property.Key] = JsonNode.Parse(JsonSerializer.Serialize(value));
                    return;
                }
            }
        }

        throw new ArgumentException($"Property '{key}' was not found.");
    }

    public static T GetValue<T>(this JsonNode jsonNode, string key)
    {
        var foundNode = FindNodeIgnoreCase(jsonNode, key);
        if (foundNode != null)
        {
            return JsonSerializer.Deserialize<T>(foundNode.ToJsonString());
        }

        throw new ArgumentNullException($"Cannot find {key} property in JSON node {jsonNode}");
    }

    private static JsonNode FindNodeIgnoreCase(JsonNode jsonNode, string key)
    {
        if (jsonNode is JsonObject jsonObject)
        {
            foreach (var property in jsonObject)
            {
                if (string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value;
                }
            }
        }

        return null;
    }
}
