namespace Darp.Utils.CodeMirror;

using System.Threading.Channels;
using System.Web;
using Avalonia;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.CodeAnalysis;
using WebViewControl;

/// <summary> The CodeMirror Editor </summary>
public sealed class CodeMirrorEditor : WebView
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(100);
    private readonly Lock _updateEditorTextLock = new();
    private readonly Channel<string> _textBoxToEditorChannel;

    /// <summary> Defines the <see cref="EditorText"/> property. </summary>
    public static readonly StyledProperty<string> EditorTextProperty = AvaloniaProperty.Register<
        CodeMirrorEditor,
        string
    >(nameof(EditorText), defaultValue: string.Empty, defaultBindingMode: BindingMode.TwoWay);

    /// <summary> Defines the <see cref="IsEditorReadOnly"/> property. </summary>
    public static readonly StyledProperty<bool> IsEditorReadOnlyProperty = AvaloniaProperty.Register<
        CodeMirrorEditor,
        bool
    >(nameof(IsEditorReadOnly), defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    /// <summary> Defines the <see cref="IsEditorLoaded"/> property. </summary>
    public static readonly DirectProperty<CodeMirrorEditor, bool> IsEditorLoadedProperty =
        AvaloniaProperty.RegisterDirect<CodeMirrorEditor, bool>(
            nameof(IsEditorLoaded),
            editor => editor.IsEditorLoaded
        );

    /// <summary> Gets or sets the text of the editor </summary>
    public string EditorText
    {
        get => GetValue(EditorTextProperty);
        set => SetValue(EditorTextProperty, value);
    }

    /// <summary> Gets or sets whether the editor is readonly </summary>
    public bool IsEditorReadOnly
    {
        get => GetValue(IsEditorReadOnlyProperty);
        set => SetValue(IsEditorReadOnlyProperty, value);
    }

    /// <summary> Gets or sets the language of the editor </summary>
    public CodeMirrorLanguage EditorLanguage
    {
        get =>
            EvaluateScript<string>("getMsLanguage", timeout: DefaultTimeout)
                .ContinueWith(task => StringToCodeMirrorLanguage(task.Result), TaskScheduler.Default)
                .GetAwaiter()
                .GetResult();
        set => ExecuteScriptFunction("setMsLanguage", CodeMirrorLanguageToString(value));
    }

    /// <summary> True, if the WebView successfully navigated to the Editor Page </summary>
    public bool IsEditorLoaded
    {
        get;
        private set => SetAndRaise(IsEditorLoadedProperty, ref field, value);
    }

    /// <summary> Initializes a new CodeMirror Editor </summary>
    /// <remarks> To actually show the editor, navigate to the address where the editor view is hosted </remarks>
    public CodeMirrorEditor()
    {
        var options = new BoundedChannelOptions(1)
        {
            SingleReader = true,
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.DropOldest,
        };
        _textBoxToEditorChannel = Channel.CreateBounded<string>(options);
        ActualThemeVariantChanged += (_, _) =>
        {
            SetEditorTheme(ActualThemeVariant == ThemeVariant.Dark ? "dark" : "light");
        };
        Navigated += (url, _) =>
        {
            if (!url.EndsWith("index.html", StringComparison.InvariantCulture))
                return;
            OnEditorNavigation();
        };
        EditorTextProperty.Changed.Subscribe(new CodeMirrorEditorObserver<string>(OnEditorPropertyChanged));
        IsEditorReadOnlyProperty.Changed.Subscribe(new CodeMirrorEditorObserver<bool>(OnEditorReadOnlyChanged));
        _ = PumpChannelAsync();
        return;

        static void OnEditorPropertyChanged(CodeMirrorEditor editor, string newText)
        {
            // The lock is set when the textChanged callback is called by the JS code
            // We don't want to trigger in that case
            if (editor._updateEditorTextLock.IsHeldByCurrentThread)
                return;
            editor._textBoxToEditorChannel.Writer.TryWrite(newText);
        }

        static void OnEditorReadOnlyChanged(CodeMirrorEditor editor, bool newIsReadOnly)
        {
            editor.SetIsEditorReadOnly(newIsReadOnly);
        }
    }

    private void OnEditorNavigation()
    {
        RegisterJavascriptObject("msTextChanged", (Action<string>)OnJsTextChanged);
        SetEditorTheme(ActualThemeVariant == ThemeVariant.Dark ? "dark" : "light");
        IsEditorLoaded = true;
        return;

        void OnJsTextChanged(string newText)
        {
            var cleanedText = newText.ReplaceLineEndings();
            {
                if (Dispatcher.UIThread.CheckAccess())
                {
                    lock (_updateEditorTextLock)
                        EditorText = cleanedText;
                    return;
                }
                Dispatcher.UIThread.Invoke(() =>
                {
                    lock (_updateEditorTextLock)
                        EditorText = cleanedText;
                });
            }
        }
    }

    private async Task PumpChannelAsync()
    {
        await foreach (var script in _textBoxToEditorChannel.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            var escapedText = HttpUtility.JavaScriptStringEncode(script);
            if (Dispatcher.UIThread.CheckAccess())
            {
                await WriteScript(escapedText).ConfigureAwait(false);
                continue;
            }
            await Dispatcher
                .UIThread.InvokeAsync(async () => await WriteScript(escapedText).ConfigureAwait(false))
                .ConfigureAwait(false);
        }
        return;

        async Task WriteScript(string script)
        {
            await EvaluateScript<string?>($"""window.setMsText("{script}");""").ConfigureAwait(false);
        }
    }

    /// <summary> Set the theme of the editor </summary>
    /// <param name="theme"> The theme to set. Might be "dark" or "light" </param>
    private void SetEditorTheme(string theme) => ExecuteScript($"""window.setMsTheme("{theme}");""");

    /// <summary> Set the text of the editor </summary>
    /// <param name="isReadOnly"> If true, the editor will be set to readOnly </param>
    private void SetIsEditorReadOnly(bool isReadOnly)
    {
        var isReadOnlyString = isReadOnly ? "true" : "false";
        ExecuteScript($"window.setMsIsReadOnly({isReadOnlyString});");
    }

    private static CodeMirrorLanguage StringToCodeMirrorLanguage(string language) =>
        language switch
        {
            LanguageNames.CSharp => CodeMirrorLanguage.CSharp,
            LanguageNames.FSharp => CodeMirrorLanguage.FSharp,
            "IL" => CodeMirrorLanguage.IntermediateLanguage,
            "PHP" => CodeMirrorLanguage.PHP,
            LanguageNames.VisualBasic => CodeMirrorLanguage.VisualBasic,
            _ => throw new ArgumentOutOfRangeException(nameof(language)),
        };

    private static string CodeMirrorLanguageToString(CodeMirrorLanguage language) =>
        language switch
        {
            CodeMirrorLanguage.CSharp => LanguageNames.CSharp,
            CodeMirrorLanguage.FSharp => LanguageNames.FSharp,
            CodeMirrorLanguage.IntermediateLanguage => "IL",
            CodeMirrorLanguage.PHP => "PHP",
            CodeMirrorLanguage.VisualBasic => LanguageNames.VisualBasic,
            _ => throw new ArgumentOutOfRangeException(nameof(language)),
        };
}

file sealed class CodeMirrorEditorObserver<T>(Action<CodeMirrorEditor, T> onChange)
    : IObserver<AvaloniaPropertyChangedEventArgs<T>>
{
    private readonly Action<CodeMirrorEditor, T> _onChange = onChange;

    public void OnNext(AvaloniaPropertyChangedEventArgs<T> value) =>
        _onChange((CodeMirrorEditor)value.Sender, value.NewValue.Value);

    public void OnCompleted()
    {
        // Nop
    }

    public void OnError(Exception error)
    {
        // Nop
    }
}
