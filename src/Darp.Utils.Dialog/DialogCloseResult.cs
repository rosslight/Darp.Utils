namespace Darp.Utils.Dialog;

using System.Diagnostics.CodeAnalysis;

public readonly struct DialogCloseResult<T> where T : notnull
{
    private DialogCloseResult(bool isCloseRequested, T? result)
    {
        IsCloseRequested = isCloseRequested;
        Result = result;
    }

    [MemberNotNullWhen(true, nameof(Result))]
    public bool IsCloseRequested { get; }
    public T? Result { get; }

    public static DialogCloseResult<DialogUnit> CreateCloseRequested(bool isCloseRequested) => new(isCloseRequested, default);
    public static DialogCloseResult<T> CreateNoCloseRequested() => new(false, default);
    public static DialogCloseResult<T> CreateCloseRequested(T result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new DialogCloseResult<T>(true, result);
    }

    public static implicit operator DialogCloseResult<T>(T? result) => result is null
        ? CreateNoCloseRequested()
        : CreateCloseRequested(result);
}