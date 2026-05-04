namespace Darp.Utils.SimpleArgumentParser;

public sealed class ParseResult
{
    private readonly ParserIdentity _owner;
    private readonly ResultSlot[] _slots;

    internal ParseResult(ParserIdentity owner, ResultSlot[] slots)
    {
        _owner = owner;
        _slots = slots;
    }

    public T GetValue<T>(Argument<T> argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        ValidateArgumentAccess(argument);
        return argument.GetValue(_slots);
    }

    public T? GetValue<T>(OptionalArgument<T> argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        ValidateArgumentAccess(argument);
        return argument.GetValue(_slots);
    }

    private void ValidateArgumentAccess(IArgument argument)
    {
        if (!ReferenceEquals(argument.Owner, _owner))
            throw new ArgumentException("The argument does not belong to this parse result.", nameof(argument));

        if (argument.Slot < 0 || argument.Slot >= _slots.Length)
            throw new ArgumentException(
                "The argument was not registered when this parse result was created.",
                nameof(argument)
            );
    }
}
