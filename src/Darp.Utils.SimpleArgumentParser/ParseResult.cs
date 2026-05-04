namespace Darp.Utils.SimpleArgumentParser;

using System.Diagnostics.CodeAnalysis;

public sealed class ParseResult
{
    private readonly object _owner;
    private readonly Dictionary<ArgumentDefinition, object?> _values;

    internal ParseResult(
        object owner,
        ParseStatus status,
        IReadOnlyList<ParseDiagnostic> diagnostics,
        Dictionary<ArgumentDefinition, object?> values
    )
    {
        _owner = owner;
        Status = status;
        Diagnostics = diagnostics;
        _values = values;
    }

    public ParseStatus Status { get; }

    public IReadOnlyList<ParseDiagnostic> Diagnostics { get; }

    public T GetValue<T>(Argument<T> argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        ValidateArgumentAccess(argument);
        return TryGetValue<T>(argument.Definition, out var value) ? value! : default!;
    }

    public T? GetValue<T>(OptionalArgument<T> argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        ValidateArgumentAccess(argument);
        return TryGetValue<T>(argument.Definition, out var value) ? value : default;
    }

    private bool TryGetValue<T>(ArgumentDefinition definition, [MaybeNull] out T value)
    {
        if (_values.TryGetValue(definition, out var rawValue))
        {
            if (rawValue is null)
            {
                value = default;
                return true;
            }

            value = (T)rawValue;
            return true;
        }

        value = default;
        return false;
    }

    private void ValidateArgumentAccess(IArgumentHandle argument)
    {
        if (Status != ParseStatus.Success)
            throw new InvalidOperationException(
                "Argument values can only be read when parsing completed successfully."
            );

        if (!ReferenceEquals(argument.Owner, _owner))
            throw new ArgumentException("The argument does not belong to this parse result.", nameof(argument));
    }
}
