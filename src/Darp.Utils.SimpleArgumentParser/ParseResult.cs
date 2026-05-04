namespace Darp.Utils.SimpleArgumentParser;

public sealed class ParseResult
{
    private readonly ParserIdentity _owner;
    private readonly ResultSlot[] _slots;

    internal ParseResult(
        ParserIdentity owner,
        ParseStatus status,
        IReadOnlyList<ParseDiagnostic> diagnostics,
        ResultSlot[] slots
    )
    {
        _owner = owner;
        Status = status;
        Diagnostics = diagnostics;
        _slots = slots;
    }

    public ParseStatus Status { get; }

    public IReadOnlyList<ParseDiagnostic> Diagnostics { get; }

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
        if (Status != ParseStatus.Success)
            throw new InvalidOperationException(
                "Argument values can only be read when parsing completed successfully."
            );

        if (!ReferenceEquals(argument.Owner, _owner))
            throw new ArgumentException("The argument does not belong to this parse result.", nameof(argument));

        if (argument.Slot < 0 || argument.Slot >= _slots.Length)
            throw new ArgumentException(
                "The argument was not registered when this parse result was created.",
                nameof(argument)
            );
    }
}
