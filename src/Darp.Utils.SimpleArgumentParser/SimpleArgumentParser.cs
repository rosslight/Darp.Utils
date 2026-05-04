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

    public OptionalArgument<T> AddOption<T>(string name, string? description = null)
        where T : ISpanParsable<T> => AddOption<T>(name, T.TryParse, description);

    public Argument<T> AddOption<T>(string name, T defaultValue, string? description = null)
        where T : ISpanParsable<T> => AddOption(name, T.TryParse, defaultValue, description);

    public OptionalArgument<T> AddOption<T>(string name, ArgumentValueParser<T> parser, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(parser);

        return RegisterNamedArgument(
            new OptionalArgument<T>(_owner, NormalizeOptionName(name), NormalizeDescription(description), parser)
        );
    }

    public Argument<T> AddOption<T>(
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
                ArgumentKind.Option,
                NormalizeOptionName(name),
                NormalizeDescription(description),
                parser,
                OptionalValue<T>.Some(defaultValue)
            )
        );
    }

    public Argument<T> AddPositional<T>(string name, string? description = null)
        where T : ISpanParsable<T> => AddPositional<T>(name, T.TryParse, description);

    public Argument<T> AddPositional<T>(string name, T defaultValue, string? description = null)
        where T : ISpanParsable<T> => AddPositional(name, T.TryParse, defaultValue, description);

    public Argument<T> AddPositional<T>(string name, ArgumentValueParser<T> parser, string? description = null)
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

    public ParseResult Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        List<ParseDiagnostic>? diagnostics = null;
        var slots = CreateResultSlots();
        var positionalIndex = 0;
        var stopParsingOptions = false;

        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i].AsSpan();

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
                var argument = FindNamedArgument(optionName);
                if (argument is null)
                {
                    var optionNameString = optionName.ToString();
                    AddDiagnostic(
                        ref diagnostics,
                        new ParseDiagnostic(
                            ParseDiagnosticKind.UnknownOption,
                            $"Unknown option '--{optionNameString}'.",
                            optionNameString
                        )
                    );
                    continue;
                }

                if (argument.Kind is ArgumentKind.Flag)
                {
                    ReadOnlySpan<char> flagValue = "true";

                    if (hasExplicitValue)
                    {
                        flagValue = explicitValue;
                    }
                    else if (i + 1 < args.Length && TryParseBool(args[i + 1].AsSpan()))
                    {
                        flagValue = args[++i].AsSpan();
                    }

                    if (!argument.TrySetValue(flagValue, FormatProvider, slots[argument.Slot]))
                        AddInvalidValueDiagnostic(ref diagnostics, argument.Name, flagValue, argument.ValueTypeName);

                    continue;
                }

                if (!hasExplicitValue)
                {
                    if (i + 1 >= args.Length || LooksLikeOption(args[i + 1].AsSpan()))
                    {
                        AddDiagnostic(
                            ref diagnostics,
                            new ParseDiagnostic(
                                ParseDiagnosticKind.MissingValue,
                                $"Option '--{argument.Name}' requires a value.",
                                argument.Name
                            )
                        );
                        continue;
                    }

                    explicitValue = args[++i].AsSpan();
                }

                if (!argument.TrySetValue(explicitValue, FormatProvider, slots[argument.Slot]))
                    AddInvalidValueDiagnostic(ref diagnostics, argument.Name, explicitValue, argument.ValueTypeName);

                continue;
            }

            ParsePositional(token, positionalIndex++, slots, ref diagnostics);
        }

        for (var index = positionalIndex; index < _positionalsInOrder.Count; index++)
        {
            var argument = _positionalsInOrder[index];
            if (argument.HasDefaultValue)
                continue;

            AddDiagnostic(
                ref diagnostics,
                new ParseDiagnostic(
                    ParseDiagnosticKind.MissingPositional,
                    $"Missing positional argument '{argument.Name}'.",
                    argument.Name
                )
            );
        }

        return new ParseResult(
            _owner,
            diagnostics is null ? ParseStatus.Success : ParseStatus.Failed,
            diagnostics ?? [],
            slots
        );
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

    private void ParsePositional(
        ReadOnlySpan<char> value,
        int positionalIndex,
        ResultSlot[] slots,
        ref List<ParseDiagnostic>? diagnostics
    )
    {
        if (positionalIndex >= _positionalsInOrder.Count)
        {
            AddDiagnostic(
                ref diagnostics,
                new ParseDiagnostic(
                    ParseDiagnosticKind.UnexpectedPositional,
                    $"Unexpected positional argument '{value.ToString()}'."
                )
            );
            return;
        }

        var argument = _positionalsInOrder[positionalIndex];
        if (!argument.TrySetValue(value, FormatProvider, slots[argument.Slot]))
            AddInvalidValueDiagnostic(ref diagnostics, argument.Name, value, argument.ValueTypeName);
    }

    private static void AddInvalidValueDiagnostic(
        ref List<ParseDiagnostic>? diagnostics,
        string argumentName,
        ReadOnlySpan<char> value,
        string valueTypeName
    )
    {
        AddDiagnostic(
            ref diagnostics,
            new ParseDiagnostic(
                ParseDiagnosticKind.InvalidValue,
                $"Argument '{argumentName}' has value '{value.ToString()}', which could not be parsed as {valueTypeName}.",
                argumentName
            )
        );
    }

    private static void AddDiagnostic(ref List<ParseDiagnostic>? diagnostics, ParseDiagnostic diagnostic)
    {
        diagnostics ??= [];
        diagnostics.Add(diagnostic);
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
