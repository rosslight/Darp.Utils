namespace Darp.Utils.SimpleArgumentParser;

public interface IArgumentDefinition;

internal sealed class ArgumentDefinition : IArgumentDefinition
{
    private readonly ArgumentDefinitionParser _parser;

    private ArgumentDefinition(
        ArgumentDefinitionKind kind,
        string name,
        string? description,
        Type valueType,
        ArgumentDefinitionParser parser,
        bool hasDefaultValue,
        object? defaultValue
    )
    {
        Kind = kind;
        Name = name;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ValueType = valueType;
        _parser = parser;
        HasDefaultValue = hasDefaultValue;
        DefaultValue = defaultValue;
    }

    public ArgumentDefinitionKind Kind { get; }

    public string Name { get; }

    public string? Description { get; }

    public Type ValueType { get; }

    public bool HasDefaultValue { get; }

    public object? DefaultValue { get; }

    public static ArgumentDefinition CreateOption<T>(
        string name,
        string? description,
        SimpleArgumentParser.ArgumentValueParser<T> parser,
        bool hasDefaultValue,
        T defaultValue
    ) => Create(ArgumentDefinitionKind.Option, name, description, parser, hasDefaultValue, defaultValue);

    public static ArgumentDefinition CreateFlag(string name, string? description) =>
        new(
            ArgumentDefinitionKind.Flag,
            name,
            description,
            typeof(bool),
            static (ReadOnlySpan<char> value, IFormatProvider? provider, out object? result) =>
            {
                if (bool.TryParse(value, out var parsedValue))
                {
                    result = parsedValue;
                    return true;
                }

                result = null;
                return false;
            },
            hasDefaultValue: true,
            defaultValue: false
        );

    public static ArgumentDefinition CreatePositional<T>(
        string name,
        string? description,
        SimpleArgumentParser.ArgumentValueParser<T> parser,
        bool hasDefaultValue,
        T defaultValue
    ) => Create(ArgumentDefinitionKind.Positional, name, description, parser, hasDefaultValue, defaultValue);

    public bool TryParseValue(ReadOnlySpan<char> rawValue, IFormatProvider? provider, out object? value) =>
        _parser(rawValue, provider, out value);

    private static ArgumentDefinition Create<T>(
        ArgumentDefinitionKind kind,
        string name,
        string? description,
        SimpleArgumentParser.ArgumentValueParser<T> parser,
        bool hasDefaultValue,
        T defaultValue
    )
    {
        return new ArgumentDefinition(
            kind,
            name,
            description,
            typeof(T),
            TryParseDefinitionValue,
            hasDefaultValue,
            hasDefaultValue ? defaultValue : null
        );

        bool TryParseDefinitionValue(ReadOnlySpan<char> value, IFormatProvider? provider, out object? result)
        {
            try
            {
                if (parser(value, provider, out T? parsedValue))
                {
                    result = parsedValue;
                    return true;
                }
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
            result = null;
            return false;
        }
    }
}
