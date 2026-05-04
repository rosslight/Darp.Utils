namespace Darp.Utils.SimpleArgumentParser;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

public sealed class SimpleArgumentParser(string? description = null)
{
    private static readonly IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

    private readonly ParserIdentity _owner = new();
    private readonly List<IArgument> _arguments = [];
    private readonly HashSet<string> _namedArgumentNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IArgument> _namedArgumentsInOrder = [];
    private readonly List<IArgument> _positionalsInOrder = [];

    public string? Description { get; } = NormalizeDescription(description);

    public delegate bool ArgumentValueParser<T>(
        ReadOnlySpan<char> value,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out T result
    );

    public Argument<bool> AddFlag(string name, string? description = null)
    {
        return RegisterNamedArgument(
            new Argument<bool>(
                _owner,
                ArgumentKind.Flag,
                NormalizeOptionName(name),
                NormalizeDescription(description),
                ParseBoolValue,
                OptionalValue<bool>.Some(false)
            )
        );
    }

    public OptionalArgument<T> AddNamed<T>(string name, ArgumentValueParser<T> parser, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(parser);
        return RegisterNamedArgument(
            new OptionalArgument<T>(
                _owner,
                ArgumentKind.Named,
                NormalizeOptionName(name),
                NormalizeDescription(description),
                parser
            )
        );
    }

    public Argument<T> AddNamed<T>(
        string name,
        ArgumentValueParser<T> parser,
        T defaultValue,
        string? description = null
    )
    {
        ArgumentNullException.ThrowIfNull(parser);
        return RegisterNamedArgument(
            new Argument<T>(
                _owner,
                ArgumentKind.Named,
                NormalizeOptionName(name),
                NormalizeDescription(description),
                parser,
                OptionalValue<T>.Some(defaultValue)
            )
        );
    }

    public Argument<T> AddRequiredNamed<T>(string name, ArgumentValueParser<T> parser, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(parser);
        return RegisterNamedArgument(
            new Argument<T>(
                _owner,
                ArgumentKind.Named,
                NormalizeOptionName(name),
                NormalizeDescription(description),
                parser,
                OptionalValue<T>.None
            )
        );
    }

    public Argument<T> AddPositional<T>(
        string name,
        ArgumentValueParser<T> parser,
        T defaultValue,
        string? description = null
    )
    {
        ArgumentNullException.ThrowIfNull(parser);
        return RegisterPositionalArgument(
            new Argument<T>(
                _owner,
                ArgumentKind.Positional,
                NormalizePositionalName(name),
                NormalizeDescription(description),
                parser,
                OptionalValue<T>.Some(defaultValue)
            )
        );
    }

    public Argument<T> AddPositional<T>(string name, T defaultValue, string? description = null)
        where T : ISpanParsable<T> => AddPositional(name, T.TryParse, defaultValue, description);

    public OptionalArgument<T> AddPositional<T>(string name, string? description = null)
        where T : ISpanParsable<T> => AddPositional<T>(name, T.TryParse, description);

    public OptionalArgument<T> AddPositional<T>(string name, ArgumentValueParser<T> parser, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(parser);
        return RegisterPositionalArgument(
            new OptionalArgument<T>(
                _owner,
                ArgumentKind.Positional,
                NormalizePositionalName(name),
                NormalizeDescription(description),
                parser
            )
        );
    }

    public Argument<T> AddRequiredPositional<T>(string name, ArgumentValueParser<T> parser, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(parser);
        return RegisterPositionalArgument(
            new Argument<T>(
                _owner,
                ArgumentKind.Positional,
                NormalizePositionalName(name),
                NormalizeDescription(description),
                parser,
                OptionalValue<T>.None
            )
        );
    }

    public bool TryParse(
        string[] args,
        [NotNullWhen(true)] out ParseResult? result,
        [NotNullWhen(false)] out string? error
    )
    {
        ArgumentNullException.ThrowIfNull(args);

        error = null;
        var slots = CreateResultSlots();
        var positionalIndex = 0;
        var stopParsingOptions = false;
        var cursor = new ArgumentCursor(args);

        while (cursor.TryRead(out ReadOnlySpan<char> token))
        {
            if (!stopParsingOptions && token.Equals("--", StringComparison.Ordinal))
            {
                stopParsingOptions = true;
                continue;
            }

            if (
                !stopParsingOptions
                && TryGetOptionToken(token, out var optionName, out var explicitValue, out var hasExplicitValue)
            )
            {
                IArgument? argument = FindNamedArgument(optionName);
                if (argument is null)
                {
                    var optionNameString = optionName.ToString();
                    return Fail($"Unknown option '--{optionNameString}'.", out result, out error);
                }

                if (argument.Kind is ArgumentKind.Flag)
                {
                    ReadOnlySpan<char> flagValue = "true";

                    if (hasExplicitValue)
                        flagValue = explicitValue;
                    else if (cursor.TryPeek(out ReadOnlySpan<char> nextToken) && TryParseBool(nextToken))
                        cursor.TryConsumeNext(out flagValue);

                    if (!argument.TrySetValue(flagValue, FormatProvider, slots[argument.Slot]))
                    {
                        return FailInvalidValue(
                            argument.Name,
                            flagValue,
                            argument.ValueTypeName,
                            out result,
                            out error
                        );
                    }

                    continue;
                }

                if (!hasExplicitValue)
                {
                    if (!cursor.TryPeek(out var nextToken) || LooksLikeOption(nextToken))
                        return Fail($"Option '--{argument.Name}' requires a value.", out result, out error);

                    cursor.TryConsumeNext(out explicitValue);
                }

                if (!argument.TrySetValue(explicitValue, FormatProvider, slots[argument.Slot]))
                {
                    return FailInvalidValue(
                        argument.Name,
                        explicitValue,
                        argument.ValueTypeName,
                        out result,
                        out error
                    );
                }

                continue;
            }

            if (!TryParsePositional(token, positionalIndex++, slots, out result, out error))
                return false;
        }

        for (var index = positionalIndex; index < _positionalsInOrder.Count; index++)
        {
            IArgument argument = _positionalsInOrder[index];
            if (!argument.IsRequired)
                continue;

            return Fail($"Missing positional argument '{argument.Name}'.", out result, out error);
        }

        result = new ParseResult(_owner, slots);
        error = null;
        return true;
    }

    private ResultSlot[] CreateResultSlots()
    {
        var slots = new ResultSlot[_arguments.Count];

        for (var index = 0; index < _arguments.Count; index++)
            slots[index] = _arguments[index].CreateResultSlot();

        return slots;
    }

    private TArgument RegisterNamedArgument<TArgument>(TArgument argument)
        where TArgument : IArgument
    {
        if (!_namedArgumentNames.Add(argument.Name))
            throw new ArgumentException(
                $"An option named '--{argument.Name}' has already been added.",
                nameof(argument)
            );

        RegisterArgument(argument);
        _namedArgumentsInOrder.Add(argument);
        return argument;
    }

    private TArgument RegisterPositionalArgument<TArgument>(TArgument argument)
        where TArgument : IArgument
    {
        RegisterArgument(argument);
        _positionalsInOrder.Add(argument);
        return argument;
    }

    private void RegisterArgument(IArgument argument)
    {
        argument.Slot = _arguments.Count;
        _arguments.Add(argument);
    }

    private IArgument? FindNamedArgument(ReadOnlySpan<char> optionName)
    {
        foreach (var argument in _namedArgumentsInOrder)
        {
            if (optionName.Equals(argument.Name.AsSpan(), StringComparison.OrdinalIgnoreCase))
                return argument;
        }

        return null;
    }

    private bool TryParsePositional(
        ReadOnlySpan<char> value,
        int positionalIndex,
        ResultSlot[] slots,
        out ParseResult? result,
        [NotNullWhen(false)] out string? error
    )
    {
        if (positionalIndex >= _positionalsInOrder.Count)
        {
            return Fail($"Unexpected positional argument '{value.ToString()}'.", out result, out error);
        }

        var argument = _positionalsInOrder[positionalIndex];
        if (!argument.TrySetValue(value, FormatProvider, slots[argument.Slot]))
            return FailInvalidValue(argument.Name, value, argument.ValueTypeName, out result, out error);

        result = null;
        error = null;
        return true;
    }

    private static bool FailInvalidValue(
        string argumentName,
        ReadOnlySpan<char> value,
        string valueTypeName,
        out ParseResult? result,
        out string? error
    )
    {
        return Fail(
            $"Argument '{argumentName}' has value '{value.ToString()}', which could not be parsed as {valueTypeName}.",
            out result,
            out error
        );
    }

    private static bool Fail(string message, out ParseResult? result, out string error)
    {
        result = null;
        error = message;
        return false;
    }

    private static bool TryGetOptionToken(
        ReadOnlySpan<char> token,
        out ReadOnlySpan<char> optionName,
        out ReadOnlySpan<char> explicitValue,
        out bool hasExplicitValue
    )
    {
        optionName = default;
        explicitValue = default;
        hasExplicitValue = false;

        if (!token.StartsWith("--", StringComparison.Ordinal) || token.Length <= 2)
            return false;

        var optionBody = token[2..];
        var equalsIndex = optionBody.IndexOf('=');
        if (equalsIndex < 0)
        {
            optionName = optionBody;
            return optionName.Length > 0;
        }

        optionName = optionBody[..equalsIndex];
        explicitValue = optionBody[(equalsIndex + 1)..];
        hasExplicitValue = true;
        return optionName.Length > 0;
    }

    private static bool LooksLikeOption(ReadOnlySpan<char> value) =>
        value.StartsWith("--", StringComparison.Ordinal) && value.Length > 2;

    private static bool TryParseBool(ReadOnlySpan<char> value) => bool.TryParse(value, out _);

    private static bool ParseBoolValue(ReadOnlySpan<char> value, IFormatProvider? provider, out bool result) =>
        bool.TryParse(value, out result);

    private ref struct ArgumentCursor(string[] args)
    {
        private int _index;

        public bool TryRead(out ReadOnlySpan<char> token)
        {
            if (_index >= args.Length)
            {
                token = default;
                return false;
            }

            token = args[_index++].AsSpan();
            return true;
        }

        public bool TryPeek(out ReadOnlySpan<char> token)
        {
            if (_index >= args.Length)
            {
                token = default;
                return false;
            }

            token = args[_index].AsSpan();
            return true;
        }

        public bool TryConsumeNext(out ReadOnlySpan<char> token)
        {
            if (!TryPeek(out token))
                return false;

            _index++;
            return true;
        }
    }

    private static string NormalizeOptionName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Option name cannot be empty.", nameof(name));

        name = name.Trim();
        while (name.StartsWith('-'))
            name = name[1..];

        if (name.Length == 0)
            throw new ArgumentException("Option name cannot be only dashes.", nameof(name));

        return name;
    }

    private static string NormalizePositionalName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Positional argument name cannot be empty.", nameof(name));

        name = name.Trim();
        if (name.StartsWith('-'))
            throw new ArgumentException("Positional argument names cannot start with dashes.", nameof(name));

        return name;
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
