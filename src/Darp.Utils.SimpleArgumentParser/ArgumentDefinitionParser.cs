namespace Darp.Utils.SimpleArgumentParser;

internal delegate bool ArgumentDefinitionParser(
    ReadOnlySpan<char> value,
    IFormatProvider? provider,
    out object? result
);
