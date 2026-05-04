namespace Darp.Utils.SimpleArgumentParser;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

public sealed class SimpleArgumentParser(string[] args)
{
    private readonly string[] _args = args ?? throw new ArgumentNullException(nameof(args));
    private CancellationTokenSource? _cancelSource;

    public bool GetFlag(string name) => TryGetValue(name, out bool value) && value;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValue<T>(string name, T? defaultValue = default)
        where T : IParsable<T> => TryGetValue<T>(name, out T? value) ? value : defaultValue;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValue<T>(string name, Func<string, T> parseFunc, T? defaultValue = default) =>
        TryGetValue(name, parseFunc, out var value) ? value : defaultValue;

    public T GetRequiredValue<T>(string name)
        where T : IParsable<T> => TryGetValue<T>(name, out T? value) ? value : throw MissingArgument(name);

    public T GetRequiredValue<T>(string name, Func<string, T> parseFunc) =>
        TryGetValue(name, parseFunc, out var value) ? value : throw MissingArgument(name);

    public CancellationToken RegisterGracefulCancellation()
    {
        if (_cancelSource is not null)
            return _cancelSource.Token;
        _cancelSource = new CancellationTokenSource();
        Console.CancelKeyPress += OnCancelKeyPress;
        return _cancelSource.Token;

        void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            // First Ctrl+C: cancel gracefully.
            // Second Ctrl+C: allow normal process termination.
            if (_cancelSource is null || _cancelSource.IsCancellationRequested)
                return;

            e.Cancel = true;
            _cancelSource.Cancel();
        }
    }

    private bool TryGetValue<T>(string name, [MaybeNullWhen(false)] out T value)
        where T : IParsable<T>
    {
        if (!TryGetRawValue<T>(name, out var rawValue))
        {
            value = default;
            return false;
        }
        if (T.TryParse(rawValue, CultureInfo.InvariantCulture, out value))
            return true;
        throw new FormatException(
            $"Argument '--{NormalizeName(name)}' has value '{rawValue}', which could not be parsed as {typeof(T).Name}."
        );
    }

    private bool TryGetValue<T>(string name, Func<string, T> parseFunc, out T value)
    {
        ArgumentNullException.ThrowIfNull(parseFunc);

        if (!TryGetRawValue<T>(name, out var rawValue))
        {
            value = default!;
            return false;
        }

        try
        {
            value = parseFunc(rawValue);
            return true;
        }
        catch (Exception ex)
        {
            throw new FormatException(
                $"Argument '--{NormalizeName(name)}' has value '{rawValue}', "
                    + $"which could not be parsed as {typeof(T).Name}.",
                ex
            );
        }
    }

    private bool TryGetRawValue<T>(string name, [NotNullWhen(true)] out string? value)
    {
        var normalizedName = NormalizeName(name);

        value = null;
        var found = false;

        for (var i = 0; i < _args.Length; i++)
        {
            var arg = _args[i];
            if (arg == "--")
                break;

            var equalsIndex = arg.IndexOf('=', StringComparison.Ordinal);
            if (equalsIndex > 0)
            {
                var argName = arg[..equalsIndex];
                if (IsNameMatch(argName, normalizedName))
                {
                    value = arg[(equalsIndex + 1)..];
                    found = true;
                }

                continue;
            }

            if (!IsNameMatch(arg, normalizedName))
                continue;

            if (typeof(T) == typeof(bool))
            {
                if (i + 1 < _args.Length && bool.TryParse(_args[i + 1], out _))
                {
                    value = _args[++i];
                }
                else
                {
                    value = "true";
                }

                found = true;
                continue;
            }

            if (i + 1 >= _args.Length || LooksLikeOption(_args[i + 1]))
            {
                throw new FormatException($"Argument '--{normalizedName}' requires a value.");
            }

            value = _args[++i];
            found = true;
        }

        return found;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Argument name cannot be empty.", nameof(name));

        name = name.Trim();

        while (name.StartsWith('-'))
            name = name[1..];

        if (name.Length == 0)
            throw new ArgumentException("Argument name cannot be only dashes.", nameof(name));

        return name;
    }

    private static bool IsNameMatch(string arg, string normalizedName)
    {
        string argName;

        if (arg.StartsWith("--", StringComparison.Ordinal))
            argName = arg[2..];
        else if (arg.StartsWith('-'))
            argName = arg[1..];
        else
            return false;

        return string.Equals(argName, normalizedName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeOption(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        if (value.StartsWith("--", StringComparison.Ordinal))
            return value.Length > 2;
        if (!value.StartsWith('-'))
            return false;

        // Allows negative numbers like -1 and -0.5 as values.
        return value.Length > 1 && !char.IsDigit(value[1]) && value[1] != '.';
    }

    private static InvalidOperationException MissingArgument(string name) =>
        new($"Missing required argument '--{NormalizeName(name)}'.");
}
