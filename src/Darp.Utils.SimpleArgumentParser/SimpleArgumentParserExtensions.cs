namespace Darp.Utils.SimpleArgumentParser;

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
}
