#nullable enable

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beter.Feed.TestingSandbox.Common.Serialization.Converters;

/// <summary>
/// Reads a value typed <see cref="object"/> as plain .NET values: a string, a whole number
/// as <see cref="long"/>, any other number as <see cref="double"/>, a boolean, null, a
/// dictionary or a list. Both hub protocols write those back unchanged; a
/// <see cref="JsonElement"/> left in a model would not survive the MessagePack protocol.
/// </summary>
public sealed class PlainObjectConverter : JsonConverter<object>
{
    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        return ValueOf(document.RootElement);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        if (value.GetType() == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    private static object? ValueOf(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject().ToDictionary(property => property.Name, property => ValueOf(property.Value), StringComparer.Ordinal),
        JsonValueKind.Array => element.EnumerateArray().Select(ValueOf).ToList(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt64(out var whole) ? whole : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => throw new JsonException(string.Create(CultureInfo.InvariantCulture, $"Unsupported JSON value kind {element.ValueKind}.")),
    };
}
