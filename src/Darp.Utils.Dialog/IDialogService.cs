namespace Darp.Utils.Dialog;

/// <summary> Provides access to all dialog specific methods. </summary>
public interface IDialogService : IDisposable
{
    /// <summary> Create a new dialog builder based on a ContentDialog </summary>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="content"> The content of the dialog </param>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    IContentDialogBuilder<TContent> CreateContentDialog<TContent>(string title, TContent content);

    /// <summary> Create a new <see cref="IDialogService"/> with information about the dialog parent attached </summary>
    /// <param name="dialogRootType"> The type of the dialog root </param>
    /// <returns> A new dialogService </returns>
    IDialogService WithDialogRoot(Type dialogRootType);
}
