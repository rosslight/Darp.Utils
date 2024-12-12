namespace Darp.Utils.TestRail.Json;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary> Convert an integer to a bool </summary>
public sealed class BoolIntJsonConverter : JsonConverter<bool>
{
    /// <inheritdoc />
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetInt32();
        return value > 0;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteNumberValue(value ? 1 : 0);
    }
}
