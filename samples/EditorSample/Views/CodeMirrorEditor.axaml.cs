namespace EditorSample.Views;

using System.Text.Json;
using System.Web;
using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
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

public partial class CodeMirrorEditor : WebView
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(100);

    public static readonly StyledProperty<string> EditorTextProperty = AvaloniaProperty.Register<
        CodeMirrorEditor,
        string
    >(nameof(EditorText), defaultValue: string.Empty, defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<CodeMirrorEditor, bool> IsEditorLoadedProperty =
        AvaloniaProperty.RegisterDirect<CodeMirrorEditor, bool>(
            nameof(IsEditorLoaded),
            editor => editor.IsEditorLoaded
        );

    public string EditorText
    {
        get => GetValue(EditorTextProperty);
        set => SetValue(EditorTextProperty, value);
    }

    private static void OnTextChanging(CodeMirrorEditor sender, bool before)
    {
        if (!before && sender.IsEditorLoaded)
            sender.SetEditorText(sender.EditorText);
    }

    /// <summary> Gets or sets the language of the editor </summary>
    public CodeMirrorLanguage EditorLanguage
    {
        get => GetEditorLanguageAsync().GetAwaiter().GetResult();
        set => ExecuteScriptFunction("setMsLanguage", CodeMirrorLanguageToString(value));
    }

    public bool IsEditorLoaded
    {
        get;
        private set => SetAndRaise(IsEditorLoadedProperty, ref field, value);
    }

    public CodeMirrorEditor()
    {
        InitializeComponent();
        EditorTextProperty.Changed.Subscribe(
            new CodeMirrorEditorObserver((editor, newText) => editor.SetEditorText(newText))
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
                var x = JsonSerializer.Deserialize<string>($"\"{newText}\"") ?? string.Empty;
                DebugLog("OnTextChanged", x);
                Dispatcher.UIThread.Post(() => EditorText = x);
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
            .ContinueWith(task => StringToCodeMirrorLanguage(task.Result));

    /// <summary> Set the text of the editor </summary>
    /// <param name="text"> The text to set </param>
    public void SetEditorText(string text)
    {
        var escapedText = HttpUtility.JavaScriptStringEncode(text);
        return;
        ExecuteScript($"""window.setMsText("{escapedText}");""");
    }

    /// <summary> Get the current text inside the editor </summary>
    /// <returns> A task which completes with the text </returns>
    public Task<string> GetEditorTextAsync() => EvaluateScript<string>("getMsText", timeout: DefaultTimeout);

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
