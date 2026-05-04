namespace Darp.Utils.SimpleArgumentParser;

internal interface IArgumentHandle
{
    object Owner { get; }

    ArgumentDefinition Definition { get; }
}
