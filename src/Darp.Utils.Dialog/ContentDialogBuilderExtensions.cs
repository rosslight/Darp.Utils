namespace Darp.Utils.Dialog;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DialogData;

/// <summary> Extensions of the <see cref="IContentDialogBuilder{TContent}"/> </summary>
public static class ContentDialogBuilderExtensions
{
    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Primary"/> button and sets the text to be displayed.
    /// Provides a synchronous onClick function
    /// </summary>
    /// <param name="builder"> The parent <see cref="IContentDialogBuilder{TContent}"/> </param>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    public static IContentDialogBuilder<TContent> SetPrimaryButton<TContent>(
        this IContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, bool>? onClick = null
    )
        where TContent : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.SetPrimaryButton(
            text,
            isEnabled,
            onClick is null ? null : (content, _) => Task.FromResult(onClick(content))
        );
    }

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Primary"/> button and sets the text to be displayed.
    /// Provides an asynchronous onClick function without <see cref="CancellationToken"/>
    /// </summary>
    /// <param name="builder"> The parent <see cref="IContentDialogBuilder{TContent}"/> </param>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    public static IContentDialogBuilder<TContent> SetPrimaryButton<TContent>(
        this IContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, Task<bool>>? onClick = null
    )
        where TContent : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.SetPrimaryButton(text, isEnabled, onClick is null ? null : (content, _) => onClick(content));
    }

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Secondary"/> button and sets the text to be displayed.
    /// Provides a synchronous onClick function
    /// </summary>
    /// <param name="builder"> The parent <see cref="IContentDialogBuilder{TContent}"/> </param>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    public static IContentDialogBuilder<TContent> SetSecondaryButton<TContent>(
        this IContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, bool>? onClick = null
    )
        where TContent : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.SetSecondaryButton(
            text,
            isEnabled,
            onClick is null ? null : (content, _) => Task.FromResult(onClick(content))
        );
    }

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Secondary"/> button and sets the text to be displayed.
    /// Provides an asynchronous onClick function without <see cref="CancellationToken"/>
    /// </summary>
    /// <param name="builder"> The parent <see cref="IContentDialogBuilder{TContent}"/> </param>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    public static IContentDialogBuilder<TContent> SetSecondaryButton<TContent>(
        this IContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, Task<bool>>? onClick = null
    )
        where TContent : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.SetSecondaryButton(text, isEnabled, onClick is null ? null : (content, _) => onClick(content));
    }

    /// <summary> Configure the input field </summary>
    /// <param name="builder"> The input dialog builder </param>
    /// <param name="watermark"> An optional watermark on the input </param>
    /// <param name="isPassword"> If true, the input field is set up to hold a password </param>
    /// <param name="validateInput"> A validation callback called on input </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static IContentDialogBuilder<InputDialogViewModel> ConfigureInput(
        this IContentDialogBuilder<InputDialogViewModel> builder,
        string? watermark = null,
        bool? isPassword = null,
        Func<string?, ValidationResult?>? validateInput = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (watermark is not null)
        {
            builder.Content.InputWatermark = watermark;
        }
        if (isPassword is not null)
        {
            builder.Content.IsPasswordInput = isPassword.Value;
        }
        if (validateInput is not null)
        {
            builder.Content.ValidateInputCallback = validateInput;
        }
        return builder;
    }

    /// <summary> Configure the input field </summary>
    /// <param name="builder"> The input dialog builder </param>
    /// <param name="usernameMessage"> The optional message to be shown on top of the input </param>
    /// <param name="validateUsername"> A func to validate the username on input </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static IContentDialogBuilder<UsernamePasswordViewModel> ConfigureUsernameStep(
        this IContentDialogBuilder<UsernamePasswordViewModel> builder,
        string usernameMessage,
        Func<string, ValidationResult?>? validateUsername = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Content.EnterUsernameMessage = usernameMessage;
        if (validateUsername is not null)
        {
            builder.Content.ValidateUsernameHandler = validateUsername;
        }
        return builder;
    }

    /// <summary> Configure the input field </summary>
    /// <param name="builder"> The input dialog builder </param>
    /// <param name="passwordMessage"> The optional message to be shown on top of the input </param>
    /// <param name="validatePassword"> A func to validate the password on input </param>
    /// <param name="checkHandler"> If present, sets <see cref="UsernamePasswordViewModel.CheckHandler"/> </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static IContentDialogBuilder<UsernamePasswordViewModel> ConfigurePasswordStep(
        this IContentDialogBuilder<UsernamePasswordViewModel> builder,
        string passwordMessage,
        Func<string, ValidationResult?>? validatePassword = null,
        UsernamePasswordViewModel.ValidatePasswordAsync? checkHandler = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Content.EnterPasswordMessage = passwordMessage;
        if (validatePassword is not null)
        {
            builder.Content.ValidatePasswordHandler = validatePassword;
        }
        if (checkHandler is not null)
        {
            builder.Content.CheckHandler = checkHandler;
        }
        return builder;
    }
}
