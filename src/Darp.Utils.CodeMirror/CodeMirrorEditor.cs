namespace Darp.Utils.CodeMirror;

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

    /// <summary> Defines the <see cref="EditorText"/> property. </summary>
    public static readonly StyledProperty<string> EditorTextProperty = AvaloniaProperty.Register<
        CodeMirrorEditor,
        string
    >(nameof(EditorText), defaultValue: string.Empty, defaultBindingMode: BindingMode.TwoWay);

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
        EditorTextProperty.Changed.Subscribe(new CodeMirrorEditorObserver(OnEditorPropertyChanged));
        return;

        static void OnEditorPropertyChanged(CodeMirrorEditor editor, string newText)
        {
            // The lock is set when the textChanged callback is called by the JS code
            // We don't want to trigger in that case
            if (editor._updateEditorTextLock.IsHeldByCurrentThread)
                return;
            editor.SetEditorText(newText);
        }
    }

    private void OnEditorNavigation()
    {
        RegisterJavascriptObject("msTextChanged", (Action<string>)OnJsTextChanged);
        SetEditorTheme(ActualThemeVariant == ThemeVariant.Dark ? "dark" : "light");
        SetEditorText(EditorText);
        IsEditorLoaded = true;
        return;

        void OnJsTextChanged(string newText)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                lock (_updateEditorTextLock)
                    EditorText = newText;
            });
        }
    }

    private void SetEditorTheme(string theme) => ExecuteScript($"""window.setMsTheme("{theme}");""");

    /// <summary> Set the text of the editor </summary>
    /// <param name="text"> The text to set </param>
    private void SetEditorText(string text)
    {
        lock (_updateEditorTextLock)
        {
            var escapedText = HttpUtility.JavaScriptStringEncode(text);
            ExecuteScript($"""window.setMsText("{escapedText}");""");
        }
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

file sealed class CodeMirrorEditorObserver(Action<CodeMirrorEditor, string> onChange)
    : IObserver<AvaloniaPropertyChangedEventArgs<string>>
{
    private readonly Action<CodeMirrorEditor, string> _onChange = onChange;

    public void OnNext(AvaloniaPropertyChangedEventArgs<string> value) =>
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
