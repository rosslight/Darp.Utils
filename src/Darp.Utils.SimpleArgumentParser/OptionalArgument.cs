namespace Darp.Utils.SimpleArgumentParser;

public sealed class OptionalArgument<T> : IArgumentHandle
{
    private readonly object _owner;
    private readonly ArgumentDefinition _definition;

    internal OptionalArgument(object owner, ArgumentDefinition definition)
    {
        _owner = owner;
        _definition = definition;
    }

    public string Name => _definition.Name;

    public string? Description => _definition.Description;

    object IArgumentHandle.Owner => _owner;

    ArgumentDefinition IArgumentHandle.Definition => _definition;

    internal ArgumentDefinition Definition => _definition;
}
