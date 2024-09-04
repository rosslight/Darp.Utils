namespace Darp.Utils.Dialog;

using System.Diagnostics.CodeAnalysis;
using FluentAvalonia.UI.Controls;

public readonly struct ContentDialogResult<TPrimary, TSecondary>(ContentDialogResult result,
    TPrimary? primary,
    TSecondary? secondary)
    where TPrimary : notnull where TSecondary : notnull
{
    private readonly ContentDialogResult _result = result;

    public bool IsNone => _result is ContentDialogResult.None;
    [MemberNotNullWhen(true, nameof(Primary))]
    public bool IsPrimary => _result is ContentDialogResult.Primary;
    [MemberNotNullWhen(true, nameof(Secondary))]
    public bool IsSecondary => _result is ContentDialogResult.Secondary;

    public TPrimary? Primary { get; } = primary;
    public TSecondary? Secondary { get; } = secondary;
}