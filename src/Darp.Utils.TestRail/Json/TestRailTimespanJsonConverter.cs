namespace Darp.Utils.TestRail.Json;

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary> Convert a timespan to the testrail format </summary>
public sealed class TestRailTimespanJsonConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var timeSpanString = reader.GetString();
        if (TimeSpan.TryParse(timeSpanString, null, out TimeSpan timeSpan))
        {
            return timeSpan;
        }

        if (timeSpanString is null)
        {
            return TimeSpan.Zero;
        }

        var split = timeSpanString.Split(" ");
        var seconds = 0;
        var minutes = 0;
        var hours = 0;
        var days = 0;
        var weeks = 0;
        foreach (var s in split)
        {
            if (s.EndsWith('s'))
            {
                seconds = int.Parse(s[..^1], CultureInfo.InvariantCulture);
            }
            else if (s.EndsWith('m'))
            {
                minutes = int.Parse(s[..^1], CultureInfo.InvariantCulture);
            }
            else if (s.EndsWith('h'))
            {
                hours = int.Parse(s[..^1], CultureInfo.InvariantCulture);
            }
            else if (s.EndsWith('d'))
            {
                days = int.Parse(s[..^1], CultureInfo.InvariantCulture);
            }
            else if (s.EndsWith('w'))
            {
                weeks = int.Parse(s[..^1], CultureInfo.InvariantCulture);
            }
        }
        return new TimeSpan(days + (weeks * 7), hours, minutes, seconds);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var seconds = value.Seconds;
        var minutes = value.Minutes;
        var hours = value.Hours;
        var days = value.Days % 7;
        var weeks = value.Days / 7;
        var stringBuilder = new StringBuilder();
        if (seconds > 0)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $" {seconds}s");
        }
        if (minutes > 0)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $" {minutes}m");
        }
        if (hours > 0)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $" {hours}h");
        }
        if (days > 0)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $" {days}d");
        }
        if (weeks > 0)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $" {weeks}w");
        }

        var result = stringBuilder.ToString().Trim();
        if (string.IsNullOrEmpty(result))
        {
            result = "1s";
        }
        writer.WriteStringValue(result);
    }
}
