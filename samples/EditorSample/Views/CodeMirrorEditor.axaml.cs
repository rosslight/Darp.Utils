namespace EditorSample.Views;

using Avalonia;
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

    /// <summary> Gets or sets the language of the editor </summary>
    public CodeMirrorLanguage EditorLanguage
    {
        get => GetEditorLanguageAsync().GetAwaiter().GetResult();
        set => ExecuteScriptFunction("setMsLanguage", CodeMirrorLanguageToString(value));
    }

    public bool IsLoaded { get; private set; }

    public CodeMirrorEditor()
    {
        InitializeComponent();
        ActualThemeVariantChanged += (_, _) =>
        {
            SetEditorTheme(ActualThemeVariant == ThemeVariant.Dark ? "dark" : "light");
        };
        Navigated += (url, _) =>
        {
            if (!url.EndsWith("index.html", StringComparison.InvariantCulture))
                return;
            IsLoaded = true;
            SetEditorTheme(ActualThemeVariant == ThemeVariant.Dark ? "dark" : "light");
        };
    }

    private void SetEditorTheme(string theme) => ExecuteScript($"""window.setMsTheme("{theme}");""");

    private Task<CodeMirrorLanguage> GetEditorLanguageAsync() =>
        EvaluateScript<string>("getMsLanguage", timeout: DefaultTimeout)
            .ContinueWith(task => StringToCodeMirrorLanguage(task.Result));

    /// <summary> Set the text of the editor </summary>
    /// <param name="text"> The text to set </param>
    public void SetEditorText(string text) => ExecuteScriptFunction("setMsText", text);

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
