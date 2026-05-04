namespace Darp.Utils.SimpleArgumentParser;

internal enum ArgumentKind
{
    Flag,
    Named,
    Positional,
}

internal sealed class ParserIdentity;

internal interface IArgument
{
    ParserIdentity Owner { get; }

    int Slot { get; set; }

    string Name { get; }

    string? Description { get; }

    ArgumentKind Kind { get; }

    bool IsRequired { get; }

    string ValueTypeName { get; }

    ResultSlot CreateResultSlot();

    bool TrySetValue(ReadOnlySpan<char> value, IFormatProvider? provider, ResultSlot slot);
}

internal abstract class ResultSlot
{
    internal abstract bool HasValue { get; }
}

internal sealed class ResultSlot<T> : ResultSlot
{
    internal OptionalValue<T> Value { get; set; }

    internal override bool HasValue => Value.HasValue;
}

internal readonly struct OptionalValue<T>
{
    private readonly T _value;

    private OptionalValue(T value)
    {
        _value = value;
        HasValue = true;
    }

    internal bool HasValue { get; }

    internal T Value => HasValue ? _value : throw new InvalidOperationException("No argument value is available.");

    internal static OptionalValue<T> None => default;

    internal static OptionalValue<T> Some(T value) => new(value);

    internal T? GetValueOrDefault() => HasValue ? _value : default;
}
