namespace Darp.Utils.SimpleArgumentParser;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

public sealed class SimpleArgumentParser(string? description = null)
{
    private static readonly IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

    private readonly object _owner = new();
    private readonly Dictionary<string, ArgumentDefinition> _namedDefinitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ArgumentDefinition> _namedDefinitionsInOrder = [];
    private readonly List<ArgumentDefinition> _positionalsInOrder = [];

    public string? Description { get; } = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    public delegate bool ArgumentValueParser<T>(
        ReadOnlySpan<char> value,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out T result
    );

    public OptionalArgument<T> AddOption<T>(string name, string? description = null)
        where T : ISpanParsable<T> => AddOption<T>(name, parser: T.TryParse, description);

    public Argument<T> AddOption<T>(string name, T defaultValue, string? description = null)
        where T : ISpanParsable<T> => AddOption(name, parser: T.TryParse, defaultValue, description);

    public OptionalArgument<T> AddOption<T>(string name, ArgumentValueParser<T> parser, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(parser);

        var definition = ArgumentDefinition.CreateOption<T>(
            NormalizeOptionName(name),
            description,
            parser,
            hasDefaultValue: false,
            default!
        );
        RegisterNamedDefinition(definition);
        return new OptionalArgument<T>(_owner, definition);
    }

    public Argument<T> AddOption<T>(
        string name,
        ArgumentValueParser<T> parser,
        T defaultValue,
        string? description = null
    )
    {
        ArgumentNullException.ThrowIfNull(parser);

        var definition = ArgumentDefinition.CreateOption<T>(
            NormalizeOptionName(name),
            description,
            parser,
            hasDefaultValue: true,
            defaultValue
        );
        RegisterNamedDefinition(definition);
        return new Argument<T>(_owner, definition);
    }

    public Argument<bool> AddFlag(string name, string? description = null)
    {
        var definition = ArgumentDefinition.CreateFlag(NormalizeOptionName(name), description);
        RegisterNamedDefinition(definition);
        return new Argument<bool>(_owner, definition);
    }

    public OptionalArgument<T> AddPositional<T>(string name, string? description = null)
        where T : ISpanParsable<T>
    {
        var definition = ArgumentDefinition.CreateOption<T>(
            NormalizePositionalName(name),
            description,
            T.TryParse,
            hasDefaultValue: false,
            default!
        );
        _positionalsInOrder.Add(definition);
        return new OptionalArgument<T>(_owner, definition);
    }

    public Argument<T> AddPositional<T>(string name, T defaultValue, string? description = null)
        where T : ISpanParsable<T>
    {
        var definition = ArgumentDefinition.CreatePositional<T>(
            NormalizePositionalName(name),
            description,
            T.TryParse,
            hasDefaultValue: true,
            defaultValue
        );
        _positionalsInOrder.Add(definition);
        return new Argument<T>(_owner, definition);
    }

    public Argument<T> AddPositional<T>(string name, ArgumentValueParser<T> parser, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(parser);

        var definition = ArgumentDefinition.CreatePositional<T>(
            NormalizePositionalName(name),
            description,
            parser,
            hasDefaultValue: false,
            default!
        );
        _positionalsInOrder.Add(definition);
        return new Argument<T>(_owner, definition);
    }

    public Argument<T> AddPositional<T>(
        string name,
        ArgumentValueParser<T> parser,
        T defaultValue,
        string? description = null
    )
    {
        ArgumentNullException.ThrowIfNull(parser);

        var definition = ArgumentDefinition.CreatePositional<T>(
            NormalizePositionalName(name),
            description,
            parser,
            hasDefaultValue: true,
            defaultValue
        );
        _positionalsInOrder.Add(definition);
        return new Argument<T>(_owner, definition);
    }

    public ParseResult Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        List<ParseDiagnostic>? diagnostics = null;
        var values = CreateDefaultValues();
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
                var definition = FindNamedDefinition(optionName);
                if (definition is null)
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

                if (definition.Kind is ArgumentDefinitionKind.Flag)
                {
                    var flagValue = true;

                    if (hasExplicitValue)
                    {
                        if (!TryParseBool(explicitValue, out flagValue))
                        {
                            AddInvalidValueDiagnostic(ref diagnostics, definition.Name, explicitValue, typeof(bool));
                            continue;
                        }
                    }
                    else if (i + 1 < args.Length && TryParseBool(args[i + 1].AsSpan(), out var nextFlagValue))
                    {
                        flagValue = nextFlagValue;
                        i++;
                    }

                    values[definition] = flagValue;
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
                                $"Option '--{definition.Name}' requires a value.",
                                definition.Name
                            )
                        );
                        continue;
                    }

                    explicitValue = args[++i].AsSpan();
                }

                if (!definition.TryParseValue(explicitValue, FormatProvider, out var optionValue))
                {
                    AddInvalidValueDiagnostic(ref diagnostics, definition.Name, explicitValue, definition.ValueType);
                    continue;
                }

                values[definition] = optionValue;
                continue;
            }

            ParsePositional(token, positionalIndex++, values, ref diagnostics);
        }

        for (var index = positionalIndex; index < _positionalsInOrder.Count; index++)
        {
            var definition = _positionalsInOrder[index];
            if (definition.HasDefaultValue)
                continue;

            AddDiagnostic(
                ref diagnostics,
                new ParseDiagnostic(
                    ParseDiagnosticKind.MissingPositional,
                    $"Missing positional argument '{definition.Name}'.",
                    definition.Name
                )
            );
        }

        return new ParseResult(
            _owner,
            diagnostics is null ? ParseStatus.Success : ParseStatus.Failed,
            diagnostics ?? [],
            values
        );
    }

    private void RegisterNamedDefinition(ArgumentDefinition definition)
    {
        if (!_namedDefinitions.TryAdd(definition.Name, definition))
            throw new ArgumentException(
                $"An option named '--{definition.Name}' has already been added.",
                nameof(definition)
            );

        _namedDefinitionsInOrder.Add(definition);
    }

    private Dictionary<ArgumentDefinition, object?> CreateDefaultValues()
    {
        var values = new Dictionary<ArgumentDefinition, object?>(
            _namedDefinitionsInOrder.Count + _positionalsInOrder.Count
        );

        foreach (var definition in _namedDefinitionsInOrder)
        {
            if (definition.HasDefaultValue)
                values[definition] = definition.DefaultValue;
        }

        foreach (var definition in _positionalsInOrder)
        {
            if (definition.HasDefaultValue)
                values[definition] = definition.DefaultValue;
        }

        return values;
    }

    private ArgumentDefinition? FindNamedDefinition(ReadOnlySpan<char> optionName)
    {
        foreach (var definition in _namedDefinitionsInOrder)
        {
            if (optionName.Equals(definition.Name.AsSpan(), StringComparison.OrdinalIgnoreCase))
                return definition;
        }

        return null;
    }

    private void ParsePositional(
        ReadOnlySpan<char> value,
        int positionalIndex,
        Dictionary<ArgumentDefinition, object?> values,
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

        var definition = _positionalsInOrder[positionalIndex];
        if (!definition.TryParseValue(value, FormatProvider, out var positionalValue))
        {
            AddInvalidValueDiagnostic(ref diagnostics, definition.Name, value, definition.ValueType);
            return;
        }

        values[definition] = positionalValue;
    }

    private static void AddInvalidValueDiagnostic(
        ref List<ParseDiagnostic>? diagnostics,
        string argumentName,
        ReadOnlySpan<char> value,
        Type valueType
    )
    {
        AddDiagnostic(
            ref diagnostics,
            new ParseDiagnostic(
                ParseDiagnosticKind.InvalidValue,
                $"Argument '{argumentName}' has value '{value.ToString()}', which could not be parsed as {valueType.Name}.",
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

    private static bool TryParseBool(ReadOnlySpan<char> value, out bool result) => bool.TryParse(value, out result);

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
}
