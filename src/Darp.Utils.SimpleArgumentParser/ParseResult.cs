namespace Darp.Utils.SimpleArgumentParser;

/// <summary>
/// Provides typed access to values produced by a successful parse.
/// </summary>
public sealed class ParseResult
{
    private readonly ParserIdentity _owner;
    private readonly ResultSlot[] _slots;

    internal ParseResult(ParserIdentity owner, ResultSlot[] slots)
    {
        _owner = owner;
        _slots = slots;
    }

    /// <summary>
    /// Gets the parsed value for a required or defaulted argument.
    /// </summary>
    /// <param name="argument">The argument handle returned when the argument was registered.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>The parsed value, or the argument default when no value was supplied.</returns>
    /// <exception cref="ArgumentException">Thrown when the argument belongs to another parser or was registered after this result was created.</exception>
    public T GetValue<T>(Argument<T> argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        ValidateArgumentAccess(argument);
        return argument.GetValue(_slots);
    }

    /// <summary>
    /// Gets the parsed value for an optional argument.
    /// </summary>
    /// <param name="argument">The argument handle returned when the argument was registered.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>The parsed value, or <see langword="null"/> when the argument was not supplied.</returns>
    /// <exception cref="ArgumentException">Thrown when the argument belongs to another parser or was registered after this result was created.</exception>
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
        {
            throw new ArgumentException(
                "The argument was not registered when this parse result was created.",
                nameof(argument)
            );
        }
    }
}
