namespace Darp.Utils.CodeMirror;

using System.Web;
using Avalonia;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.Threading;
using WebViewControl;

public enum CodeMirrorLanguage
{
    CSharp,
    FSharp,
    VisualBasic,
    IntermediateLanguage,
    PHP,
}

/// <summary> The CodeMirror Editor </summary>
public sealed class CodeMirrorEditor : WebView
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(100);
    private Lock _updateEditorTextLock = new();

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
        get => GetEditorLanguageAsync().GetAwaiter().GetResult();
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
        EditorTextProperty.Changed.Subscribe(
            new CodeMirrorEditorObserver(
                (editor, newText) =>
                {
                    if (_updateEditorTextLock.IsHeldByCurrentThread)
                    {
                        Console.WriteLine("Do not set!");
                        return;
                    }
                    editor.SetEditorText(newText);
                }
            )
        );
        ActualThemeVariantChanged += (_, _) =>
        {
            SetEditorTheme(ActualThemeVariant == ThemeVariant.Dark ? "dark" : "light");
        };
        Navigated += (url, _) =>
        {
            if (!url.EndsWith("index.html", StringComparison.InvariantCulture))
                return;
            RegisterJavascriptObject("msTextChanged", (Action<string>)OnTextChanged);
            SetEditorTheme(ActualThemeVariant == ThemeVariant.Dark ? "dark" : "light");
            SetEditorText(EditorText);
            IsEditorLoaded = true;
            AllowDeveloperTools = true;
            ShowDeveloperTools();
            return;

            void OnTextChanged(string newText)
            {
                DebugLog("OnTextChanged", newText);
                Dispatcher.UIThread.Invoke(() =>
                {
                    lock (_updateEditorTextLock)
                        EditorText = newText;
                });
            }
        };
    }

    public static void DebugLog(string start, string? text)
    {
        switch (text)
        {
            case null:
                Console.WriteLine($"{start} {text}");
                break;
            case "":
                Console.WriteLine($"{start} [empty]");
                break;
            default:
                Console.WriteLine(
                    $"{start} {text.Replace(" ", "[space]").Replace("\r\n", "[crlf]").Replace("\n", "[lf]")}"
                );
                break;
        }
    }

    private void SetEditorTheme(string theme) => ExecuteScript($"""window.setMsTheme("{theme}");""");

    private Task<CodeMirrorLanguage> GetEditorLanguageAsync() =>
        EvaluateScript<string>("getMsLanguage", timeout: DefaultTimeout)
            .ContinueWith(task => StringToCodeMirrorLanguage(task.Result), TaskScheduler.Default);

    /// <summary> Set the text of the editor </summary>
    /// <param name="text"> The text to set </param>
    public void SetEditorText(string text)
    {
        var escapedText = HttpUtility.JavaScriptStringEncode(text);
        ExecuteScript($"""window.setMsText("{escapedText}");""");
    }

    private static CodeMirrorLanguage StringToCodeMirrorLanguage(string language) =>
        language switch
        {
            "C#" => CodeMirrorLanguage.CSharp,
            "F#" => CodeMirrorLanguage.FSharp,
            "IL" => CodeMirrorLanguage.IntermediateLanguage,
            "PHP" => CodeMirrorLanguage.PHP,
            "VisualBasic" => CodeMirrorLanguage.VisualBasic,
            _ => throw new ArgumentOutOfRangeException(nameof(language)),
        };

    private static string CodeMirrorLanguageToString(CodeMirrorLanguage language) =>
        language switch
        {
            CodeMirrorLanguage.CSharp => "C#",
            CodeMirrorLanguage.FSharp => "F#",
            CodeMirrorLanguage.IntermediateLanguage => "IL",
            CodeMirrorLanguage.PHP => "PHP",
            CodeMirrorLanguage.VisualBasic => "VisualBasic",
            _ => throw new ArgumentOutOfRangeException(nameof(language)),
        };
}

file sealed class CodeMirrorEditorObserver(Action<CodeMirrorEditor, string> onChange)
    : IObserver<AvaloniaPropertyChangedEventArgs<string>>
{
    private readonly Action<CodeMirrorEditor, string> _onChange = onChange;

    public void OnNext(AvaloniaPropertyChangedEventArgs<string> value)
    {
        CodeMirrorEditor.DebugLog("OnTextChanged", value.NewValue.Value);
        _onChange((CodeMirrorEditor)value.Sender, value.NewValue.Value);
    }

    public void OnCompleted()
    {
        // Nop
    }

    public void OnError(Exception error)
    {
        // Nop
    }
}
