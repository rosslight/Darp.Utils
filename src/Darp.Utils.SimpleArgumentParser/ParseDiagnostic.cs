namespace Darp.Utils.SimpleArgumentParser;

public sealed record ParseDiagnostic(ParseDiagnosticKind Kind, string Message, string? ArgumentName = null);
