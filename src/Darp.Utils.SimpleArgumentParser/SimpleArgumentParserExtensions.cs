namespace Darp.Utils.SimpleArgumentParser;

using System.Diagnostics;

/// <summary>
/// Convenience methods for common parser registrations and process-exit handling.
/// </summary>
public static class SimpleArgumentParserExtensions
{
    /// <summary>
    /// Adds an optional named option parsed via <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <param name="parser">The parser to add the option to.</param>
    /// <param name="name">The case-sensitive long option name, such as <c>--count</c>.</param>
    /// <param name="description">Optional help text for the option.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle whose result value is <see langword="null"/> when the option is absent.</returns>
    public static OptionalArgument<T> AddNamed<T>(
        this SimpleArgumentParser parser,
        string name,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddNamed<T>(name, T.TryParse, description);

    /// <summary>
    /// Adds a required named option parsed via <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <param name="parser">The parser to add the option to.</param>
    /// <param name="name">The case-sensitive long option name, such as <c>--count</c>.</param>
    /// <param name="description">Optional help text for the option.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed value.</returns>
    public static Argument<T> AddRequiredNamed<T>(
        this SimpleArgumentParser parser,
        string name,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddRequiredNamed<T>(name, T.TryParse, description);

    /// <summary>
    /// Adds a named option with a default value, parsed via <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <param name="parser">The parser to add the option to.</param>
    /// <param name="name">The case-sensitive long option name, such as <c>--count</c>.</param>
    /// <param name="defaultValue">The value returned when the option is absent.</param>
    /// <param name="description">Optional help text for the option.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed or default value.</returns>
    public static Argument<T> AddNamed<T>(
        this SimpleArgumentParser parser,
        string name,
        T defaultValue,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddNamed(name, T.TryParse, defaultValue, description);

    /// <summary>
    /// Adds a required positional argument parsed via <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <param name="parser">The parser to add the argument to.</param>
    /// <param name="name">The positional argument name used in errors and result lookup.</param>
    /// <param name="description">Optional help text for the argument.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed value.</returns>
    public static Argument<T> AddRequiredPositional<T>(
        this SimpleArgumentParser parser,
        string name,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddRequiredPositional<T>(name, T.TryParse, description);

    /// <summary>
    /// Adds an optional positional argument parsed via <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <param name="parser">The parser to add the argument to.</param>
    /// <param name="name">The positional argument name used in errors and result lookup.</param>
    /// <param name="description">Optional help text for the argument.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle whose result value is <see langword="null"/> when the token is absent.</returns>
    public static OptionalArgument<T> AddPositional<T>(
        this SimpleArgumentParser parser,
        string name,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddPositional<T>(name, T.TryParse, description);

    /// <summary>
    /// Adds a positional argument with a default value, parsed via <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <param name="parser">The parser to add the argument to.</param>
    /// <param name="name">The positional argument name used in errors and result lookup.</param>
    /// <param name="defaultValue">The value returned when no token is supplied for this position.</param>
    /// <param name="description">Optional help text for the argument.</param>
    /// <typeparam name="T">The parsed value type.</typeparam>
    /// <returns>An argument handle used to read the parsed or default value.</returns>
    public static Argument<T> AddPositional<T>(
        this SimpleArgumentParser parser,
        string name,
        T defaultValue,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddPositional(name, T.TryParse, defaultValue, description);

    /// <summary>
    /// Parses the arguments, writes parse errors to <see cref="Console.Error"/>, and exits with code 1 on failure.
    /// </summary>
    /// <param name="parser">The parser to use.</param>
    /// <param name="args">The command-line tokens, usually from <c>Main</c>.</param>
    /// <returns>The parsed result when parsing succeeds.</returns>
    public static ParseResult ParseOrExit(this SimpleArgumentParser parser, string[] args)
    {
        ArgumentNullException.ThrowIfNull(parser);
        if (parser.TryParse(args, out ParseResult? result, out var error))
            return result;
        Console.Error.WriteLine(error);
        Environment.Exit(1);
        throw new UnreachableException("Environment.Exit() should have exited the process.");
    }

    /// <summary>
    /// Parses an enum value using the same delegate shape as parser registrations.
    /// </summary>
    /// <param name="value">The token text to parse.</param>
    /// <param name="_">Ignored format provider.</param>
    /// <param name="result">The parsed enum value when parsing succeeds.</param>
    /// <typeparam name="T">The enum value type.</typeparam>
    /// <returns><see langword="true"/> when the value matched an enum name or numeric value.</returns>
    public static bool TryParseEnum<T>(ReadOnlySpan<char> value, IFormatProvider? _, out T result)
        where T : struct => Enum.TryParse(value, out result);
}
