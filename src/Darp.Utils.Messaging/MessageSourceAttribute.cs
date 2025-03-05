namespace Darp.Utils.Messaging;

/// <summary> Marks a message source for the source generator </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class MessageSourceAttribute : Attribute;
