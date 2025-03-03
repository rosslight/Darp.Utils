namespace Darp.Utils.Messaging.Generator;

using Microsoft.CodeAnalysis;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor GeneralError = new(
        id: "DMGO001",
        title: "GeneralError",
        messageFormat: "Failed to generate code for this object because of {0}",
        category: "DarpMessagingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidMethodParameters = new(
        id: "DMGO002",
        title: "InvalidMethodArguments",
        messageFormat: "Method has invalid syntax. Only a single parameter is allowed.",
        category: "DarpMessagingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidMethodTypeParameters = new(
        id: "DMGO003",
        title: "InvalidMethodTypeParameters",
        messageFormat: "Method has invalid syntax. Only a single type parameter is allowed.",
        category: "DarpMessagingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidMethodTypeConstraint = new(
        id: "DMGO004",
        title: "InvalidMethodTypeConstraint",
        messageFormat: "Method has invalid constraints. Only a single constraint where 'T : allows ref struct' is allowed.",
        category: "DarpMessagingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
