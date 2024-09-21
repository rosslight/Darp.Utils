namespace Darp.Utils.ResxSourceGenerator;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;

internal sealed class Impl(ResourceInformation resourceInformation)
{
    private const int MaxDocCommentLength = 256;

    public ResourceInformation ResourceInformation { get; } = resourceInformation;
    public CompilationInformation CompilationInformation => ResourceInformation.CompilationInformation;

    public string? OutputTextHintName { get; private set; }
    public SourceText OutputText { get; private set; } = SourceText.From("", Encoding.UTF8);

    private static readonly string[] Separator = ["\r\n", "\r", "\n"];

    private void LogError(string message)
    {
        OutputText = SourceText.From($"#error {message}", Encoding.UTF8, SourceHashAlgorithm.Sha256);
    }

    [MemberNotNullWhen(true, nameof(OutputTextHintName), nameof(OutputText))]
    public bool Execute(CancellationToken cancellationToken)
    {
        OutputTextHintName = ResourceInformation.ResourceHintName + $".Designer.cs";

        if (string.IsNullOrEmpty(ResourceInformation.ResourceName))
        {
            LogError("ResourceName not specified");
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
            LogError("ResourceFile was null");
            return false;
        }

        var strings = new StringBuilder();
        using var sourceTextReader = new SourceTextReader(text);
        foreach (XElement? node in XDocument.Load(sourceTextReader).Descendants("data"))
        {
            var name = node.Attribute("name")?.Value;
            if (name == null)
            {
                LogError("Missing resource name");
                return false;
            }

            var value = node.Elements("value").FirstOrDefault()?.Value.Trim();
            if (value == null)
            {
                LogError($"Missing resource value: '{name}'");
                return false;
            }

            if (name.Length == 0)
            {
                LogError("Empty resource name");
                return false;
            }

            var docCommentString = value.Length > MaxDocCommentLength ? value[..MaxDocCommentLength] + " ..." : value;

            RenderDocComment(memberIndent, strings, docCommentString);

            var identifier = GetIdentifierFromResourceName(name);

            strings.AppendLine($"{memberIndent}public static string @{identifier} => GetResourceString(\"{name}\")!;");

            if (ResourceInformation.EmitFormatMethods)
            {
                var resourceString = new ResourceString(name, value);

                if (resourceString.HasArguments)
                {
                    RenderDocComment(memberIndent, strings, docCommentString);
                    RenderFormatMethod(memberIndent, strings, resourceString);
                }
            }
        }

        string? getStringMethod;
        if (ResourceInformation.OmitGetResourceString)
        {
            getStringMethod = null;
        }
        else
        {
            var getResourceStringAttributes = new List<string>();
            if (CompilationInformation.HasAggressiveInlining)
            {
                getResourceStringAttributes.Add("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
            }

            if (CompilationInformation.HasNotNullIfNotNull)
            {
                getResourceStringAttributes.Add("[return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull(\"defaultValue\")]");
            }

            getStringMethod = $@"{memberIndent}public static global::System.Globalization.CultureInfo? Culture {{ get; set; }}
{string.Join(Environment.NewLine, getResourceStringAttributes.Select(attr => memberIndent + attr))}
{memberIndent}internal static string? GetResourceString(string resourceKey, string? defaultValue = null) =>  ResourceManager.GetString(resourceKey, Culture) ?? defaultValue;";
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
        }

        string? namespaceStart, namespaceEnd;
        if (namespaceName == null)
        {
            namespaceStart = namespaceEnd = null;
        }
        else
        {
            namespaceStart = $@"namespace {namespaceName}
{{";
            namespaceEnd = "}";
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

            resourceTypeDefinition = $"{resourceClassIndent}internal static class {resourceClassName} {{ }}";
            if (resourceNamespaceName != null)
            {
                resourceTypeDefinition = $@"namespace {resourceNamespaceName}
{{
{resourceTypeDefinition}
}}";
            }
        }

        // The ResourceManager property being initialized lazily is an important optimization that lets .NETNative
        // completely remove the ResourceManager class if the disk space saving optimization to strip resources
        // (/DisableExceptionMessages) is turned on in the compiler.
        var result = $@"// <auto-generated/>

#nullable enable
using System.Reflection;

{resourceTypeDefinition}
{namespaceStart}
{classIndent}{(ResourceInformation.Public ? "public" : "internal")} static partial class {className}
{classIndent}{{
{memberIndent}private static global::System.Resources.ResourceManager? s_resourceManager;
{memberIndent}public static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof({resourceTypeName})));
{getStringMethod}
{strings}
{classIndent}}}
{namespaceEnd}
";
        OutputText = SourceText.From(result.Replace("\r\n", "\n"), Encoding.UTF8, SourceHashAlgorithm.Sha256);
        return true;
    }

    private static string GetIdentifierFromResourceName(string name)
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

    private static void RenderDocComment(string memberIndent, StringBuilder strings, string value)
    {
        var escapedTrimmedValue = new XElement("summary", value).ToString();

        foreach (var line in escapedTrimmedValue.Split(Separator, StringSplitOptions.None))
        {
            strings.Append(memberIndent)
                .Append("///")
                .Append(' ')
                .AppendLine(line);
        }
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

    private static void RenderFormatMethod(string indent, StringBuilder strings, ResourceString resourceString)
    {
        strings.AppendLine($"{indent}internal static string Format{resourceString.Name}({resourceString.GetMethodParameters()})");
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
        private static readonly Regex NamedParameterMatcher = new(@"\{([a-z]\w*)\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex NumberParameterMatcher = new(@"\{(\d+)\}", RegexOptions.Compiled);
        private readonly IReadOnlyList<string> _arguments;

        public ResourceString(string name, string value)
        {
            Name = name;
            Value = value;

            MatchCollection match = NamedParameterMatcher.Matches(value);
            UsingNamedArgs = match.Count > 0;

            if (!UsingNamedArgs)
            {
                match = NumberParameterMatcher.Matches(value);
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

        public string GetMethodParameters()
        {
            return string.Join(", ", _arguments.Select(a => "object? " + GetArgName(a)));
        }

        private string GetArgName(string name) => UsingNamedArgs ? name : 'p' + name;
    }
}
