namespace Darp.Utils.SimpleArgumentParser;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

/// <summary>
/// Parses command-line arguments into typed named and positional values.
/// </summary>
/// <param name="description">Optional text describing the command handled by this parser.</param>
public sealed class SimpleArgumentParser(string? description = null)
{
    private static readonly IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

    private readonly ParserIdentity _owner = new();
    private readonly List<IArgument> _arguments = [];
    private readonly HashSet<string> _namedArgumentNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IArgument> _namedArgumentsInOrder = [];
    private readonly List<IArgument> _positionalsInOrder = [];

    /// <summary>
    /// Gets the normalized parser description, or <see langword="null"/> when none was supplied.
    /// </summary>
    public string? Description { get; } = NormalizeDescription(description);

    /// <summary>
    /// Converts a raw argument value into a typed value.
    /// </summary>
    /// <param name="value">The command-line token text to parse.</param>
    /// <param name="provider">The format provider used by the parser.</param>
    /// <param name="result">The parsed value when parsing succeeds.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns><see langword="true"/> when the value was parsed successfully.</returns>
    public delegate bool ArgumentValueParser<T>(
        ReadOnlySpan<char> value,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out T result
    );

    /// <summary>
    /// Adds a presence-only boolean option that defaults to <see langword="false"/>.
    /// </summary>
    /// <param name="name">The option name, with or without leading dashes.</param>
    /// <param name="description">Optional help text for the option.</param>
    /// <returns>An argument handle used to read the parsed flag value.</returns>
    /// <remarks>Passing <c>--name</c> sets the value to <see langword="true"/>. Use <c>AddNamed&lt;bool&gt;</c> for explicit boolean values.</remarks>
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

    /// <summary>
    /// Adds an optional named option parsed from <c>--name value</c> or <c>--name=value</c>.
    /// </summary>
    /// <param name="name">The option name, with or without leading dashes.</param>
    /// <param name="parser">The parser used to convert the option value.</param>
    /// <param name="description">Optional help text for the option.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle whose result value is <see langword="null"/> when the option is absent.</returns>
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

    /// <summary>
    /// Adds a named option with a default value.
    /// </summary>
    /// <param name="name">The option name, with or without leading dashes.</param>
    /// <param name="parser">The parser used to convert the option value.</param>
    /// <param name="defaultValue">The value returned when the option is absent.</param>
    /// <param name="description">Optional help text for the option.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed or default value.</returns>
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

    /// <summary>
    /// Adds a named option that must be supplied for parsing to succeed.
    /// </summary>
    /// <param name="name">The option name, with or without leading dashes.</param>
    /// <param name="parser">The parser used to convert the option value.</param>
    /// <param name="description">Optional help text for the option.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed value.</returns>
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

    /// <summary>
    /// Adds a positional argument with a default value.
    /// </summary>
    /// <param name="name">The positional argument name used in errors and result lookup.</param>
    /// <param name="parser">The parser used to convert the positional value.</param>
    /// <param name="defaultValue">The value returned when no token is supplied for this position.</param>
    /// <param name="description">Optional help text for the argument.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed or default value.</returns>
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

    /// <summary>
    /// Adds a positional argument with a default value, parsed via <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <param name="name">The positional argument name used in errors and result lookup.</param>
    /// <param name="defaultValue">The value returned when no token is supplied for this position.</param>
    /// <param name="description">Optional help text for the argument.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed or default value.</returns>
    public Argument<T> AddPositional<T>(string name, T defaultValue, string? description = null)
        where T : ISpanParsable<T> => AddPositional(name, T.TryParse, defaultValue, description);

    /// <summary>
    /// Adds an optional positional argument parsed via <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <param name="name">The positional argument name used in errors and result lookup.</param>
    /// <param name="description">Optional help text for the argument.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle whose result value is <see langword="null"/> when the token is absent.</returns>
    public OptionalArgument<T> AddPositional<T>(string name, string? description = null)
        where T : ISpanParsable<T> => AddPositional<T>(name, T.TryParse, description);

    /// <summary>
    /// Adds an optional positional argument.
    /// </summary>
    /// <param name="name">The positional argument name used in errors and result lookup.</param>
    /// <param name="parser">The parser used to convert the positional value.</param>
    /// <param name="description">Optional help text for the argument.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle whose result value is <see langword="null"/> when the token is absent.</returns>
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

    /// <summary>
    /// Adds a positional argument that must be supplied for parsing to succeed.
    /// </summary>
    /// <param name="name">The positional argument name used in errors and result lookup.</param>
    /// <param name="parser">The parser used to convert the positional value.</param>
    /// <param name="description">Optional help text for the argument.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed value.</returns>
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

    /// <summary>
    /// Attempts to parse the supplied command-line tokens.
    /// </summary>
    /// <param name="args">The command-line tokens, usually from <c>Main</c>.</param>
    /// <param name="result">The parsed result when parsing succeeds.</param>
    /// <param name="error">A user-facing error message when parsing fails.</param>
    /// <returns><see langword="true"/> when all required arguments were supplied and all values were valid.</returns>
    /// <remarks>Use <c>--</c> to stop named-option parsing and treat following tokens as positional values.</remarks>
    public bool TryParse(
        string[] args,
        [NotNullWhen(true)] out ParseResult? result,
        [NotNullWhen(false)] out string? error
    )
    {
        ArgumentNullException.ThrowIfNull(args);

        error = null;
        ResultSlot[] slots = CreateResultSlots();
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
                    if (hasExplicitValue)
                        return Fail($"Option '--{argument.Name}' does not accept a value.", out result, out error);

                    ReadOnlySpan<char> flagValue = "true";
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

        if (!ValidateRequiredArguments(slots, out result, out error))
            return false;

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

    private bool ValidateRequiredArguments(
        ResultSlot[] slots,
        out ParseResult? result,
        [NotNullWhen(false)] out string? error
    )
    {
        foreach (IArgument argument in _arguments)
        {
            if (!argument.IsRequired || slots[argument.Slot].HasValue)
                continue;

            var message =
                argument.Kind is ArgumentKind.Named
                    ? $"Missing option '--{argument.Name}'."
                    : $"Missing positional argument '{argument.Name}'.";
            return Fail(message, out result, out error);
        }

        result = null;
        error = null;
        return true;
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
        foreach (IArgument argument in _namedArgumentsInOrder)
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

        IArgument argument = _positionalsInOrder[positionalIndex];
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

        ReadOnlySpan<char> optionBody = token[2..];
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
