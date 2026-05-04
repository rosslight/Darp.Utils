namespace Darp.Utils.SimpleArgumentParser;

public enum ParseDiagnosticKind
{
    UnknownOption,
    MissingValue,
    InvalidValue,
    MissingPositional,
    UnexpectedPositional,
}
