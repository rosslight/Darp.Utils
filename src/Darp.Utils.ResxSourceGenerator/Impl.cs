namespace Darp.Utils.ResxSourceGenerator;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

internal sealed class Impl(ResourceInformation resourceInformation)
{
    private const int maxDocCommentLength = 256;

    public ResourceInformation ResourceInformation { get; } = resourceInformation;
    public CompilationInformation CompilationInformation => ResourceInformation.CompilationInformation;

    public string? OutputTextHintName { get; private set; }
    public SourceText OutputText { get; private set; } = SourceText.From("", Encoding.UTF8);

    private static readonly string[] separator = ["\r\n", "\r", "\n"];

    private enum Lang
    {
        CSharp,
        VisualBasic,
    }

    private void LogError(Lang language, string message)
    {
        var result = language switch
        {
            Lang.CSharp => $"#error {message}",
            Lang.VisualBasic => $"#Error \"{message}\"",
            _ => message,
        };

        OutputText = SourceText.From(result, Encoding.UTF8, SourceHashAlgorithm.Sha256);
    }

    [MemberNotNullWhen(true, nameof(OutputTextHintName), nameof(OutputText))]
    public bool Execute(CancellationToken cancellationToken)
    {
        Lang language;
        switch (CompilationInformation.CodeLanguage)
        {
            case LanguageNames.CSharp:
                language = Lang.CSharp;
                break;

            case LanguageNames.VisualBasic:
                language = Lang.VisualBasic;
                break;

            default:
                LogError(Lang.CSharp, $"GenerateResxSource doesn't support language: '{CompilationInformation.CodeLanguage}'");
                return false;
        }

        var extension = language switch
        {
            Lang.CSharp => "cs",
            Lang.VisualBasic => "vb",
            _ => "cs",
        };

        OutputTextHintName = ResourceInformation.ResourceHintName + $".Designer.{extension}";

        if (string.IsNullOrEmpty(ResourceInformation.ResourceName))
        {
            LogError(language, "ResourceName not specified");
            return false;
        }

        var resourceAccessName = (string.IsNullOrEmpty(ResourceInformation.ResourceClassName)
            ? ResourceInformation.ResourceName
            : ResourceInformation.ResourceClassName) ?? throw new NotImplementedException();
        SplitName(resourceAccessName, out var namespaceName, out var className);

        var classIndent = namespaceName == null ? "" : "    ";
        var memberIndent = classIndent + "    ";

        SourceText? text = ResourceInformation.ResourceFile.GetText(cancellationToken);
        if (text is null)
        {
            LogError(language, "ResourceFile was null");
            return false;
        }

        var strings = new StringBuilder();
        foreach (XElement? node in XDocument.Load(new SourceTextReader(text)).Descendants("data"))
        {
            var name = node.Attribute("name")?.Value;
            if (name == null)
            {
                LogError(language, "Missing resource name");
                return false;
            }

            var value = node.Elements("value").FirstOrDefault()?.Value.Trim();
            if (value == null)
            {
                LogError(language, $"Missing resource value: '{name}'");
                return false;
            }

            if (name.Length == 0)
            {
                LogError(language, $"Empty resource name");
                return false;
            }

            var docCommentString = value.Length > maxDocCommentLength ? value[..maxDocCommentLength] + " ..." : value;

            RenderDocComment(language, memberIndent, strings, docCommentString);

            var identifier = GetIdentifierFromResourceName(name);

            var defaultValue = ResourceInformation.IncludeDefaultValues ? ", " + CreateStringLiteral(value, language) : string.Empty;

            switch (language)
            {
                case Lang.CSharp:
                    if (ResourceInformation.AsConstants)
                    {
                        strings.AppendLine($"{memberIndent}public const string @{identifier} = \"{name}\";");
                    }
                    else
                    {
                        var needSuppression = false;
                        if (CompilationInformation.SupportsNullable)
                        {
                            // We need a suppression unless default values are included and the NotNullIfNotNull
                            // attribute has been applied to eliminated the need for a suppression
                            if (!ResourceInformation.IncludeDefaultValues || !CompilationInformation.HasNotNullIfNotNull)
                                needSuppression = true;
                        }

                        strings.AppendLine($"{memberIndent}public static string @{identifier} => GetResourceString(\"{name}\"{defaultValue}){(needSuppression ? "!" : "")};");
                    }

                    if (ResourceInformation.EmitFormatMethods)
                    {
                        var resourceString = new ResourceString(name, value);

                        if (resourceString.HasArguments)
                        {
                            RenderDocComment(language, memberIndent, strings, docCommentString);
                            RenderFormatMethod(memberIndent, language, CompilationInformation.SupportsNullable, strings, resourceString);
                        }
                    }

                    break;

                case Lang.VisualBasic:
                    if (ResourceInformation.AsConstants)
                    {
                        strings.AppendLine($"{memberIndent}Public Const [{identifier}] As String = \"{name}\"");
                    }
                    else
                    {
                        strings.AppendLine($"{memberIndent}Public Shared ReadOnly Property [{identifier}] As String");
                        strings.AppendLine($"{memberIndent}  Get");
                        strings.AppendLine($"{memberIndent}    Return GetResourceString(\"{name}\"{defaultValue})");
                        strings.AppendLine($"{memberIndent}  End Get");
                        strings.AppendLine($"{memberIndent}End Property");
                    }

                    if (ResourceInformation.EmitFormatMethods)
                    {
                        throw new NotImplementedException();
                    }

                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        string? getStringMethod;
        if (ResourceInformation.OmitGetResourceString)
        {
            getStringMethod = null;
        }
        else
        {
            switch (language)
            {
                case Lang.CSharp:
                    var getResourceStringAttributes = new List<string>();
                    if (CompilationInformation.HasAggressiveInlining)
                    {
                        getResourceStringAttributes.Add("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
                    }

                    if (CompilationInformation.HasNotNullIfNotNull)
                    {
                        getResourceStringAttributes.Add("[return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull(\"defaultValue\")]");
                    }

                    getStringMethod = $@"{memberIndent}public static global::System.Globalization.CultureInfo{(CompilationInformation.SupportsNullable ? "?" : "")} Culture {{ get; set; }}
{string.Join(Environment.NewLine, getResourceStringAttributes.Select(attr => memberIndent + attr))}
{memberIndent}internal static {(CompilationInformation.SupportsNullable ? "string?" : "string")} GetResourceString(string resourceKey, {(CompilationInformation.SupportsNullable ? "string?" : "string")} defaultValue = null) =>  ResourceManager.GetString(resourceKey, Culture) ?? defaultValue;";
                    if (ResourceInformation.EmitFormatMethods)
                    {
                        getStringMethod += $@"

{memberIndent}private static string GetResourceString(string resourceKey, string[]? formatterNames)
{memberIndent}{{
{memberIndent}   var value = GetResourceString(resourceKey) ?? """";
{memberIndent}   if (formatterNames != null)
{memberIndent}   {{
{memberIndent}       for (var i = 0; i < formatterNames.Length; i++)
{memberIndent}       {{
{memberIndent}           value = value.Replace(""{{"" + formatterNames[i] + ""}}"", ""{{"" + i + ""}}"");
{memberIndent}       }}
{memberIndent}   }}
{memberIndent}   return value;
{memberIndent}}}
";
                    }

                    break;

                case Lang.VisualBasic:
                    getStringMethod = $@"{memberIndent}Public Shared Property Culture As Global.System.Globalization.CultureInfo
{memberIndent}<Global.System.Runtime.CompilerServices.MethodImpl(Global.System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
{memberIndent}Friend Shared Function GetResourceString(ByVal resourceKey As String, Optional ByVal defaultValue As String = Nothing) As String
{memberIndent}    Return ResourceManager.GetString(resourceKey, Culture)
{memberIndent}End Function";
                    if (ResourceInformation.EmitFormatMethods)
                    {
                        throw new NotImplementedException();
                    }

                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        string? namespaceStart, namespaceEnd;
        if (namespaceName == null)
        {
            namespaceStart = namespaceEnd = null;
        }
        else
        {
            switch (language)
            {
                case Lang.CSharp:
                    namespaceStart = $@"namespace {namespaceName}
{{";
                    namespaceEnd = "}";
                    break;

                case Lang.VisualBasic:
                    namespaceStart = $"Namespace Global.{namespaceName}";
                    namespaceEnd = "End Namespace";
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        string resourceTypeName;
        string? resourceTypeDefinition;
        if (string.IsNullOrEmpty(ResourceInformation.ResourceClassName) || ResourceInformation.ResourceName == ResourceInformation.ResourceClassName)
        {
            // resource name is same as accessor, no need for a second type.
            resourceTypeName = className;
            resourceTypeDefinition = null;
        }
        else
        {
            // resource name differs from the access class, need a type for specifying the resources
            // this empty type must remain as it is required by the .NETNative toolchain for locating resources
            // once assemblies have been merged into the application
            resourceTypeName = ResourceInformation.ResourceName;

            SplitName(resourceTypeName, out var resourceNamespaceName, out var resourceClassName);
            var resourceClassIndent = resourceNamespaceName == null ? "" : "    ";

            switch (language)
            {
                case Lang.CSharp:
                    resourceTypeDefinition = $"{resourceClassIndent}internal static class {resourceClassName} {{ }}";
                    if (resourceNamespaceName != null)
                    {
                        resourceTypeDefinition = $@"namespace {resourceNamespaceName}
{{
{resourceTypeDefinition}
}}";
                    }

                    break;

                case Lang.VisualBasic:
                    resourceTypeDefinition = $@"{resourceClassIndent}Friend Class {resourceClassName}
{resourceClassIndent}End Class";
                    if (resourceNamespaceName != null)
                    {
                        resourceTypeDefinition = $@"Namespace {resourceNamespaceName}
{resourceTypeDefinition}
End Namespace";
                    }

                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        // The ResourceManager property being initialized lazily is an important optimization that lets .NETNative
        // completely remove the ResourceManager class if the disk space saving optimization to strip resources
        // (/DisableExceptionMessages) is turned on in the compiler.
        var result = language switch
        {
            Lang.CSharp => $@"// <auto-generated/>

{(CompilationInformation.SupportsNullable ? "#nullable enable" : "")}
using System.Reflection;

{resourceTypeDefinition}
{namespaceStart}
{classIndent}{(ResourceInformation.Public ? "public" : "internal")} static partial class {className}
{classIndent}{{
{memberIndent}private static global::System.Resources.ResourceManager{(CompilationInformation.SupportsNullable ? "?" : "")} s_resourceManager;
{memberIndent}public static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof({resourceTypeName})));
{getStringMethod}
{strings}
{classIndent}}}
{namespaceEnd}
",
            Lang.VisualBasic => $@"' <auto-generated/>

Imports System.Reflection

{resourceTypeDefinition}
{namespaceStart}
{classIndent}{(ResourceInformation.Public ? "Public" : "Friend")} Partial Class {className}
{memberIndent}Private Sub New
{memberIndent}End Sub
{memberIndent}
{memberIndent}Private Shared s_resourceManager As Global.System.Resources.ResourceManager
{memberIndent}Public Shared ReadOnly Property ResourceManager As Global.System.Resources.ResourceManager
{memberIndent}    Get
{memberIndent}        If s_resourceManager Is Nothing Then
{memberIndent}            s_resourceManager = New Global.System.Resources.ResourceManager(GetType({resourceTypeName}))
{memberIndent}        End If
{memberIndent}        Return s_resourceManager
{memberIndent}    End Get
{memberIndent}End Property
{getStringMethod}
{strings}
{classIndent}End Class
{namespaceEnd}
",
            _ => throw new InvalidOperationException(),
        };
        OutputText = SourceText.From(result.Replace("\r\n", "\n"), Encoding.UTF8, SourceHashAlgorithm.Sha256);
        return true;
    }

    internal static string GetIdentifierFromResourceName(string name)
    {
        if (name.All(IsIdentifierPartCharacter))
        {
            return IsIdentifierStartCharacter(name[0]) ? name : "_" + name;
        }

        var builder = new StringBuilder(name.Length);

        var f = name[0];
        if (IsIdentifierPartCharacter(f) && !IsIdentifierStartCharacter(f))
        {
            builder.Append('_');
        }

        foreach (var c in name)
        {
            builder.Append(IsIdentifierPartCharacter(c) ? c : '_');
        }

        return builder.ToString();

        static bool IsIdentifierStartCharacter(char ch)
            => ch == '_' || IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));

        static bool IsIdentifierPartCharacter(char ch)
        {
            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            return IsLetterChar(cat)
                   || cat == UnicodeCategory.DecimalDigitNumber
                   || cat == UnicodeCategory.ConnectorPunctuation
                   || cat == UnicodeCategory.Format
                   || cat == UnicodeCategory.NonSpacingMark
                   || cat == UnicodeCategory.SpacingCombiningMark;
        }

        static bool IsLetterChar(UnicodeCategory cat)
        {
            switch (cat)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
                case UnicodeCategory.ClosePunctuation:
                    break;
                case UnicodeCategory.ConnectorPunctuation:
                    break;
                case UnicodeCategory.Control:
                    break;
                case UnicodeCategory.CurrencySymbol:
                    break;
                case UnicodeCategory.DashPunctuation:
                    break;
                case UnicodeCategory.DecimalDigitNumber:
                    break;
                case UnicodeCategory.EnclosingMark:
                    break;
                case UnicodeCategory.FinalQuotePunctuation:
                    break;
                case UnicodeCategory.Format:
                    break;
                case UnicodeCategory.InitialQuotePunctuation:
                    break;
                case UnicodeCategory.LineSeparator:
                    break;
                case UnicodeCategory.MathSymbol:
                    break;
                case UnicodeCategory.ModifierSymbol:
                    break;
                case UnicodeCategory.NonSpacingMark:
                    break;
                case UnicodeCategory.OpenPunctuation:
                    break;
                case UnicodeCategory.OtherNotAssigned:
                    break;
                case UnicodeCategory.OtherNumber:
                    break;
                case UnicodeCategory.OtherPunctuation:
                    break;
                case UnicodeCategory.OtherSymbol:
                    break;
                case UnicodeCategory.ParagraphSeparator:
                    break;
                case UnicodeCategory.PrivateUse:
                    break;
                case UnicodeCategory.SpaceSeparator:
                    break;
                case UnicodeCategory.SpacingCombiningMark:
                    break;
                case UnicodeCategory.Surrogate:
                    break;
                default:
                    break;
            }

            return false;
        }
    }

    private static void RenderDocComment(Lang language, string memberIndent, StringBuilder strings, string value)
    {
        var docCommentStart = language == Lang.CSharp
            ? "///"
            : "'''";

        var escapedTrimmedValue = new XElement("summary", value).ToString();

        foreach (var line in escapedTrimmedValue.Split(separator, StringSplitOptions.None))
        {
            strings.Append(memberIndent).Append(docCommentStart).Append(' ');
            strings.AppendLine(line);
        }
    }

    private static string CreateStringLiteral(string original, Lang lang)
    {
        var stringLiteral = new StringBuilder(original.Length + 3);
        if (lang == Lang.CSharp)
        {
            stringLiteral.Append('@');
        }

        stringLiteral.Append('\"');
        for (var i = 0; i < original.Length; i++)
        {
            // duplicate '"' for VB and C#
            if (original[i] == '\"')
            {
                stringLiteral.Append('"');
            }

            stringLiteral.Append(original[i]);
        }

        stringLiteral.Append('\"');

        return stringLiteral.ToString();
    }

    private static void SplitName(string fullName, out string? namespaceName, out string className)
    {
        var lastDot = fullName.LastIndexOf('.');
        if (lastDot == -1)
        {
            namespaceName = null;
            className = fullName;
        }
        else
        {
            namespaceName = fullName[..lastDot];
            className = fullName[(lastDot + 1)..];
        }
    }

    private static void RenderFormatMethod(string indent, Lang language, bool supportsNullable, StringBuilder strings, ResourceString resourceString)
    {
        strings.AppendLine($"{indent}internal static string Format{resourceString.Name}({resourceString.GetMethodParameters(language, supportsNullable)})");
        if (resourceString.UsingNamedArgs)
        {
            strings.AppendLine($@"{indent}   => string.Format(Culture, GetResourceString(""{resourceString.Name}"", new[] {{ {resourceString.GetArgumentNames()} }}), {resourceString.GetArguments()});");
        }
        else
        {
            strings.AppendLine($@"{indent}   => string.Format(Culture, GetResourceString(""{resourceString.Name}"") ?? """", {resourceString.GetArguments()});");
        }

        strings.AppendLine();
    }

    private sealed class ResourceString
    {
        private static readonly Regex _namedParameterMatcher = new(@"\{([a-z]\w*)\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _numberParameterMatcher = new(@"\{(\d+)\}", RegexOptions.Compiled);
        private readonly IReadOnlyList<string> _arguments;

        public ResourceString(string name, string value)
        {
            Name = name;
            Value = value;

            MatchCollection match = _namedParameterMatcher.Matches(value);
            UsingNamedArgs = match.Count > 0;

            if (!UsingNamedArgs)
            {
                match = _numberParameterMatcher.Matches(value);
            }

            IEnumerable<string> arguments = match.Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .Distinct();
            if (!UsingNamedArgs)
            {
                arguments = arguments.OrderBy(Convert.ToInt32);
            }

            _arguments = arguments.ToList();
        }

        public string Name { get; }

        public string Value { get; }

        public bool UsingNamedArgs { get; }

        public bool HasArguments => _arguments.Count > 0;

        public string GetArgumentNames() => string.Join(", ", _arguments.Select(a => "\"" + a + "\""));

        public string GetArguments() => string.Join(", ", _arguments.Select(GetArgName));

        public string GetMethodParameters(Lang language, bool supportsNullable)
        {
            return language switch
            {
                Lang.CSharp => string.Join(", ", _arguments.Select(a => $"object{(supportsNullable ? "?" : "")} " + GetArgName(a))),
                Lang.VisualBasic => string.Join(", ", _arguments.Select(GetArgName)),
                _ => throw new NotImplementedException(),
            };
        }

        private string GetArgName(string name) => UsingNamedArgs ? name : 'p' + name;
    }
}