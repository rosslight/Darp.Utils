namespace Darp.Utils.SimpleArgumentParser;

using System.Diagnostics;

public static class SimpleArgumentParserExtensions
{
    public static OptionalArgument<T> AddNamed<T>(
        this SimpleArgumentParser parser,
        string name,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddNamed<T>(name, T.TryParse, description);

    public static Argument<T> AddRequiredNamed<T>(
        this SimpleArgumentParser parser,
        string name,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddRequiredNamed<T>(name, T.TryParse, description);

    public static Argument<T> AddNamed<T>(
        this SimpleArgumentParser parser,
        string name,
        T defaultValue,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddNamed(name, T.TryParse, defaultValue, description);

    public static Argument<T> AddRequiredPositional<T>(
        this SimpleArgumentParser parser,
        string name,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddRequiredPositional<T>(name, T.TryParse, description);

    public static Argument<T> AddPositional<T>(
        this SimpleArgumentParser parser,
        string name,
        T defaultValue,
        string? description = null
    )
        where T : ISpanParsable<T> => parser.AddPositional(name, T.TryParse, defaultValue, description);

    /// <summary>
    /// Parses the arguments and exits the process if the parse fails.
    /// </summary>
    /// <param name="parser"> The simple parser to use for the parsing </param>
    /// <param name="args"> The arguments passed to the console </param>
    /// <returns> The result of the parsing </returns>
    public static ParseResult ParseOrExit(this SimpleArgumentParser parser, string[] args)
    {
        ArgumentNullException.ThrowIfNull(parser);
        if (parser.TryParse(args, out ParseResult? result, out var error))
            return result;
        Console.Error.WriteLine(error);
        Environment.Exit(1);
        throw new UnreachableException("Environment.Exit() should have exited the process.");
    }
}
