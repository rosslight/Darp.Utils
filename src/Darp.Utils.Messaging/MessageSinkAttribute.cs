namespace Darp.Utils.Messaging;

/// <summary> Marks a method as being a message sink. Expects method to be a void method with only a single argument. </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MessageSinkAttribute : Attribute;
