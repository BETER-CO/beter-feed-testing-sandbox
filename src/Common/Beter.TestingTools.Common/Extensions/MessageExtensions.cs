using System.Text.Json;

namespace Beter.TestingTools.Common.Extensions;

public static class MessageExtensions
{
    public static T GetValue<T>(this IDictionary<string, object> message, string key)
    {
        if (message.TryGetValue(key, out var currentValue))
        {
            if (currentValue is JsonElement jsonElement)
            {
                string rawText = jsonElement.GetRawText();
                return JsonSerializer.Deserialize<T>(rawText);
            }
            else
            {
                return (T)Convert.ChangeType(currentValue, typeof(T));
            }
        }

        throw new ArgumentNullException($"Cannot find {key} property in message {JsonSerializer.Serialize(message)}");
    }
}
