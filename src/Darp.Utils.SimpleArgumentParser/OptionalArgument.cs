namespace Darp.Utils.SimpleArgumentParser;

public sealed class OptionalArgument<T> : IArgument
{
    private readonly ParserIdentity _owner;
    private readonly ArgumentKind _kind;
    private readonly SimpleArgumentParser.ArgumentValueParser<T> _parser;
    private int _slot = -1;

    internal OptionalArgument(
        ParserIdentity owner,
        ArgumentKind kind,
        string name,
        string? description,
        SimpleArgumentParser.ArgumentValueParser<T> parser
    )
    {
        _owner = owner;
        _kind = kind;
        Name = name;
        Description = description;
        _parser = parser;
    }

    public string Name { get; }

    public string? Description { get; }

    ParserIdentity IArgument.Owner => _owner;

    int IArgument.Slot
    {
        get => _slot;
        set => _slot = value;
    }

    ArgumentKind IArgument.Kind => _kind;

    bool IArgument.IsRequired => false;

    string IArgument.ValueTypeName => typeof(T).Name;

    ResultSlot IArgument.CreateResultSlot() => new ResultSlot<T>();

    bool IArgument.TrySetValue(ReadOnlySpan<char> value, IFormatProvider? provider, ResultSlot slot)
    {
        if (!_parser(value, provider, out var result))
            return false;

        GetTypedSlot(slot).Value = OptionalValue<T>.Some(result);
        return true;
    }

    internal T? GetValue(ResultSlot[] slots) => GetTypedSlot(slots[_slot]).Value.GetValueOrDefault();

    private static ResultSlot<T> GetTypedSlot(ResultSlot slot)
    {
        if (slot is ResultSlot<T> typedSlot)
            return typedSlot;

        throw new InvalidOperationException("The result slot does not match the argument type.");
    }
}
