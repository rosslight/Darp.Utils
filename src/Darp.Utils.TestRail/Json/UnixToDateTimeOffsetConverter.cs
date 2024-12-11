namespace Darp.Utils.TestRail.Json;

using System.Text.Json;
using System.Text.Json.Serialization;

public class UnixToDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public bool IsFormatInSeconds { get; init; } = true;

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var time = reader.GetInt64();

        // if 'IsFormatInSeconds' is unspecified, then deduce the correct type based on whether it can be represented as seconds within the .net DateTime min/max range (1/1/0001 to 31/12/9999)
        // - because we're dealing with a 64bit value, the unix time in seconds can exceed the traditional 32bit min/max restrictions (1/1/1970 to 19/1/2038)
        return IsFormatInSeconds
            ? DateTimeOffset.FromUnixTimeSeconds(time)
            : DateTimeOffset.FromUnixTimeMilliseconds(time);
    }

    // write is out of scope, but this could be implemented via writer.ToUnixTimeMilliseconds/WriteNullValue
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        throw new NotSupportedException();
}
