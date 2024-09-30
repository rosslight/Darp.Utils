namespace Darp.Utils.ResxSourceGenerator;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

internal static class BuildHelper
{
    private const int MaxDocCommentLength = 256;
    private const string HelpLinkUri = "https://github.com/rosslight/Darp.Utils/tree/main/src/Darp.Utils.ResxSourceGenerator/README.md#Diagnostics";
    private static readonly DiagnosticDescriptor EmptyWarning = new(
        id: "DarpResX001",
        title: "Empty resource file",
        messageFormat: "Resource file generated without any members",
        category: "Globalization",
        defaultSeverity: DiagnosticSeverity.Warning,
        helpLinkUri: HelpLinkUri,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor InvalidKeyWarning = new(
        id: "DarpResX002",
        title: "Invalid Key in resource file",
        messageFormat: "Entry with key '{0}' is invalid and will be ignored",
        category: "Globalization",
        defaultSeverity: DiagnosticSeverity.Warning,
        helpLinkUri: HelpLinkUri,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor MissingValueWarning = new(
        id: "DarpResX003",
        title: "Missing Value in resource file",
        messageFormat: "Entry with key '{0}' is has no value and will be ignored",
        category: "Globalization",
        defaultSeverity: DiagnosticSeverity.Warning,
        helpLinkUri: HelpLinkUri,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor DuplicateKeyWarning = new(
        id: "DarpResX004",
        title: "Duplicate Key in resource file",
        messageFormat: "Entry with key '{0}' is duplicated and will be ignored",
        category: "Globalization",
        defaultSeverity: DiagnosticSeverity.Warning,
        helpLinkUri: HelpLinkUri,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor MissingTranslationKeyWarning = new(
        id: "DarpResX005",
        title: "Missing translation for specific Key",
        messageFormat: "Entry with key '{0}' is missing a translation for {1} ({2})",
        category: "Globalization",
        defaultSeverity: DiagnosticSeverity.Warning,
        helpLinkUri: HelpLinkUri,
        isEnabledByDefault: true
    );

    public static bool TryGenerateSource(ResourceCollection resourceCollection,
        in List<Diagnostic> diagnostics,
        [NotNullWhen(true)] out string? sourceCode,
        CancellationToken cancellationToken)
    {
        ResourceInformation resourceInformation = resourceCollection.BaseInformation;

        GenerateNamespaceStartAndEnd(resourceInformation.Namespace,
            out var namespaceStart,
            out var classIndent,
            out var memberIndent,
            out var namespaceEnd);
        if (!TryGenerateMembers(resourceCollection, memberIndent,
                out var members,
                out var keysMembers,
                diagnostics,
                cancellationToken))
        {
            sourceCode = null;
            return false;
        }

        string? getStringMethod = null;
        if (resourceInformation.Settings.EmitFormatMethods)
        {
            getStringMethod += $$$$"""
{{{{memberIndent}}}}private string GetResourceString(string resourceKey, string[]? formatterNames)
{{{{memberIndent}}}}{
{{{{memberIndent}}}}    var value = GetResourceString(resourceKey);
{{{{memberIndent}}}}    if (formatterNames == null) return value;
{{{{memberIndent}}}}    for (var i = 0; i < formatterNames.Length; i++)
{{{{memberIndent}}}}    {
{{{{memberIndent}}}}        value = value.Replace($"{{{formatterNames[i]}}}", $"{{{i}}}");
{{{{memberIndent}}}}    }
{{{{memberIndent}}}}    return value;
{{{{memberIndent}}}}}

""";
        }
        var defaultClass = $$"""
{{classIndent}}/// <summary>A strongly typed resource class for '{{resourceInformation.ResourceFile.Path}}'</summary>
{{classIndent}}{{(resourceInformation.Settings.Public ? "public" : "internal")}} sealed partial class {{resourceInformation.ClassName}}
{{classIndent}}{
{{memberIndent}}private static {{resourceInformation.ClassName}}? _default;
{{memberIndent}}/// <summary>The Default implementation of <see cref="{{resourceInformation.ClassName}}"/></summary>
{{memberIndent}}public static {{resourceInformation.ClassName}} Default => _default ??= new {{resourceInformation.ClassName}}();

{{memberIndent}}public delegate void CultureChangedDelegate(global::System.Globalization.CultureInfo? oldCulture, global::System.Globalization.CultureInfo? newCulture);
{{memberIndent}}/// <summary>Called after the <see cref="Culture"/> was updated. Provides previous culture and the newly set culture</summary>
{{memberIndent}}public event CultureChangedDelegate? CultureChanged;

{{memberIndent}}private global::System.Globalization.CultureInfo? _culture;
{{memberIndent}}/// <summary>Get or set the Culture to be used for all resource lookups issued by this strongly typed resource class.</summary>
{{memberIndent}}public System.Globalization.CultureInfo? Culture
{{memberIndent}}{
{{memberIndent}}    get => _culture;
{{memberIndent}}    set
{{memberIndent}}    {
{{memberIndent}}        System.Globalization.CultureInfo? oldCulture = _culture;
{{memberIndent}}        _culture = value;
{{memberIndent}}        if (!System.Collections.Generic.EqualityComparer<System.Globalization.CultureInfo>.Default.Equals(oldCulture, value))
{{memberIndent}}            CultureChanged?.Invoke(oldCulture, value);
{{memberIndent}}    }
{{memberIndent}}}

{{memberIndent}}///<summary>Returns the cached ResourceManager instance used by this class.</summary>
{{memberIndent}}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Advanced)]
{{memberIndent}}public global::System.Resources.ResourceManager ResourceManager { get; } = new global::System.Resources.ResourceManager("{{resourceInformation.ResourceName}}", typeof({{resourceInformation.ClassName}}).Assembly);

{{memberIndent}}/// <summary>Get a resource of the <see cref="ResourceManager"/> with the configured <see cref="Culture"/> as a string</summary>
{{memberIndent}}/// <param name="resourceKey">The name of the resource to get</param>
{{memberIndent}}/// <returns>Returns the resource value as a string or the <paramref name="resourceKey"/> if it could not be found</returns>
{{memberIndent}}[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
{{memberIndent}}public string GetResourceString(string resourceKey) => ResourceManager.GetString(resourceKey, Culture) ?? resourceKey;
{{getStringMethod}}
{{members}}
{{memberIndent}}/// <summary>All keys contained in <see cref="{{resourceInformation.ClassName}}"/></summary>
{{memberIndent}}public static class Keys
{{memberIndent}}{
{{keysMembers}}
{{memberIndent}}}
{{classIndent}}}
""";
        var debugInformation = resourceCollection.GenerateDebugInformation();
        var result = $"""
// <auto-generated/>
{debugInformation}
#nullable enable

{namespaceStart}
{defaultClass}
{namespaceEnd}

""";
        sourceCode = result.Replace("\r\n", "\n");
        return true;
    }

    private static string? GenerateDebugInformation(this ResourceCollection resourceCollection)
    {
        ResourceInformation resourceInformation = resourceCollection.BaseInformation;
        if (!resourceInformation.Settings.EmitDebugInformation)
            return null;
        return $"""
// Files:
// FileHintName: {resourceCollection.FileHintName}
{string.Join("\n", resourceCollection.OtherLanguages.Select(x => $"// {x.Key}: {x.Value.Path}"))}
// Configuration:
// RootNamespace: {resourceInformation.Settings.RootNamespace ?? "<null>"}
// RelativeDir: {resourceInformation.Settings.RelativeDir ?? "<null>"}
// ClassName: {resourceInformation.Settings.ClassName ?? "<null>"}
// Public: {resourceInformation.Settings.Public}
// EmitFormatMethods: {resourceInformation.Settings.EmitFormatMethods}
// Compilation info:
// Assembly: {resourceInformation.CompilationInformation.AssemblyName}
// Computed properties:
// ResourceName: {resourceInformation.ResourceName}
// ClassName: {resourceInformation.ClassName}
// Namespace: {resourceInformation.Namespace ?? "<null>"}
""";
    }

    private static bool TryGenerateMembers(ResourceCollection resourceCollection,
        string memberIndent,
        [NotNullWhen(true)] out string? members,
        [NotNullWhen(true)] out string? keysMembers,
        in List<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        ResourceInformation resourceInformation = resourceCollection.BaseInformation;
        var membersBuilder = new StringBuilder();
        var keysMembersBuilder = new StringBuilder();

        if (!resourceInformation.ResourceFile.TryGetResourceDataAndValues(diagnostics,
                out Dictionary<string, XElement>? values,
                cancellationToken))
        {
            members = keysMembers = null;
            return false;
        }
        if (values.Count == 0)
        {
            members = keysMembers = null;
            diagnostics.Add(Diagnostic.Create(descriptor: EmptyWarning,
                location: Location.Create(resourceInformation.ResourceFile.Path, default, default),
                messageArgs: null));
            return false;
        }
        Dictionary<CultureInfo, Dictionary<string, XElement>> otherCulturesEntries = [];
        foreach (KeyValuePair<CultureInfo, AdditionalText> pair in resourceCollection.OtherLanguages)
        {
            if (!pair.Value.TryGetResourceDataAndValues(diagnostics,
                    out Dictionary<string, XElement>? langSpecificValues,
                    cancellationToken))
            {
                continue;
            }
            otherCulturesEntries[pair.Key] = langSpecificValues;
        }
        foreach (KeyValuePair<string, XElement> x in values)
        {
            (var name, XElement attribute) = (x.Key, x.Value);
            var value = attribute.Value.Trim();
            var propertyIdentifier = GetIdentifierFromResourceName(name);
            membersBuilder.AppendLine($"""
{memberIndent}/// <summary>Get the resource of <see cref="Keys.@{propertyIdentifier}"/></summary>
{memberIndent}/// {GetTrimmedDocComment("value", value)}
{memberIndent}public string @{propertyIdentifier} => GetResourceString(Keys.@{propertyIdentifier});
""");
            keysMembersBuilder.AppendLine($"""
{memberIndent}    /// <summary> <list type="table">
{memberIndent}    /// <item> <term><b>Default</b></term> {GetTrimmedDocComment("description", value)} </item>
""");
            if (resourceInformation.Settings.EmitFormatMethods)
            {
                var resourceString = new ResourceString(propertyIdentifier, value);
                if (resourceString.HasArguments)
                {
                    RenderFormatMethod(memberIndent, membersBuilder, resourceString);
                }
            }

            foreach (KeyValuePair<CultureInfo, Dictionary<string, XElement>> entry in otherCulturesEntries
                         .OrderBy(item => item.Key.ToString()))
            {
                string otherValue;
                if (entry.Value.TryGetValue(name, out XElement otherAttribute))
                {
                    otherValue = otherAttribute.Value.Trim();
                }
                else
                {
                    diagnostics.Add(Diagnostic.Create(
                        descriptor: MissingTranslationKeyWarning,
                        location: Location.Create(resourceCollection.OtherLanguages[entry.Key].Path, default, default),
                        messageArgs: [name, entry.Key.DisplayName, entry.Key]));
                    otherValue = "n/a";
                }
                keysMembersBuilder.AppendLine(
                    $"{memberIndent}    /// <item> <term><b>{entry.Key}</b></term> {GetTrimmedDocComment("description", otherValue)} </item>");
            }
            keysMembersBuilder.AppendLine($"""
{memberIndent}    /// </list> </summary>
{memberIndent}    public const string @{propertyIdentifier} = @"{name}";
""");
        }
        members = membersBuilder.ToString();
        keysMembers = keysMembersBuilder.ToString().TrimEnd();
        return true;
    }
    private static void RenderFormatMethod(string indent, StringBuilder strings, ResourceString resourceString)
    {
        var propertyIdentifier = resourceString.Identifier;
        var methodParameters = resourceString.GetMethodParameters();
        var arguments = resourceString.GetJoinedArguments();
        var argumentNames = resourceString.UsingNamedArgs
            ? $"GetResourceString(@{propertyIdentifier}, new[] {{ {resourceString.GetArgumentNames()} }})"
            : $"@{propertyIdentifier}";
        var paramDocs = string.Join("\n", resourceString.GetArguments().Select((x, i) =>
            $"{indent}/// <param name=\"{x}\">The parameter to be used at position {{{i}}}</param>"));

        strings.AppendLine($"""
{indent}/// <summary>Format the resource of <see cref="Keys.@{propertyIdentifier}"/></summary>
{indent}/// {GetTrimmedDocComment("value", resourceString.Value)}
{paramDocs}
{indent}/// <returns>The formatted <see cref="Keys.@{propertyIdentifier}"/> string</returns>
{indent}public string @Format{propertyIdentifier}({methodParameters}) => string.Format(Culture, {argumentNames}, {arguments});
""");
    }

    private static bool TryGetResourceDataAndValues(this AdditionalText additionalText,
        in List<Diagnostic> diagnostics,
        [NotNullWhen(true)] out Dictionary<string, XElement>? resourceNames,
        CancellationToken cancellationToken)
    {
        SourceText? text = additionalText.GetText(cancellationToken);
        if (text is null)
        {
            diagnostics.Add(Diagnostic.Create(descriptor: EmptyWarning,
                location: Location.Create(additionalText.Path, default, default),
                messageArgs: null));
            resourceNames = null;
            return false;
        }
        using var sourceTextReader = new SourceTextReader(text);
        resourceNames = [];
        foreach (XElement node in XDocument.Load(sourceTextReader, LoadOptions.SetLineInfo).Descendants("data"))
        {
            XAttribute? nameAttribute = node.Attribute("name");
            var name = nameAttribute?.Value;
            if (nameAttribute is null || name is null || string.IsNullOrWhiteSpace(name))
            {
                diagnostics.Add(Diagnostic.Create(
                    descriptor: InvalidKeyWarning,
                    location: GetXElementLocation(additionalText, node, name),
                    messageArgs: name));
                continue;
            }
            XElement? valueAttribute = node.Elements("value").FirstOrDefault();
            if (valueAttribute is null)
            {
                diagnostics.Add(Diagnostic.Create(
                    descriptor: MissingValueWarning,
                    location: GetXElementLocation(additionalText, nameAttribute, name),
                    messageArgs: name));
                continue;
            }

            if (resourceNames.ContainsKey(name))
            {
                diagnostics.Add(Diagnostic.Create(
                    descriptor: DuplicateKeyWarning,
                    location: GetXElementLocation(additionalText, nameAttribute, name),
                    messageArgs: name));
                continue;
            }
            resourceNames[name] = valueAttribute;
        }
        return true;
    }

    private static Location GetXElementLocation(AdditionalText text, IXmlLineInfo line, string? memberName) =>
        Location.Create(filePath: text.Path, textSpan: new TextSpan(),
            lineSpan: new LinePositionSpan(
                start: new LinePosition(line.LineNumber - 1, line.LinePosition - 1),
                end: new LinePosition(line.LineNumber - 1, line.LinePosition - 1 + memberName?.Length ?? 0)
            )
        );

    private static string GetTrimmedDocComment(string elementName, string value)
    {
        var trimmedValue = value.Length > MaxDocCommentLength ? value[..MaxDocCommentLength] + " ..." : value;
        var element = new XElement(elementName, trimmedValue).ToString();
        var splits = element.Split('\n');
        return string.Join("<br/>", splits.Select(x => x.Trim()));
    }

    private static void GenerateNamespaceStartAndEnd(string? namespaceName,
        out string? namespaceStart,
        out string classIndent,
        out string memberIndent,
        out string? namespaceEnd)
    {
        const string indent = "    ";
        if (namespaceName is null)
        {
            namespaceStart = null;
            classIndent = "";
            namespaceEnd = null;
        }
        else
        {
            namespaceStart = $$"""
namespace {{namespaceName}}
{
""";
            classIndent = indent;
            namespaceEnd = "}";
        }
        memberIndent = classIndent + indent;
    }

    public static bool SplitName(string fullName,
        [NotNullWhen(true)] out string? namespaceName,
        out string className)
    {
        var lastDot = fullName.LastIndexOf('.');
        if (lastDot == -1)
        {
            namespaceName = null;
            className = fullName;
            return false;
        }
        namespaceName = fullName[..lastDot];
        className = fullName[(lastDot + 1)..];
        return true;
    }

    public delegate bool TryParseDelegate<T>(string value, [NotNullWhen(true)] out T? result)
        where T : class;
    public delegate bool TryParseDelegateStruct<T>(string value, [NotNullWhen(true)] out T result)
        where T : struct;

    public static bool? GetBoolValue(this AnalyzerConfigOptions options, string key) => options
        .GetStructValue(key, (string value, out bool result) => bool.TryParse(value, out result));
    public static T? GetStructValue<T>(this AnalyzerConfigOptions options,
        string key,
        TryParseDelegateStruct<T> tryParse)
        where T : struct
    {
        if (options.TryGetValue(key, out var stringValue)
            && tryParse(stringValue, out T value))
        {
            return value;
        }
        return null;
    }
    public static string? GetValue(this AnalyzerConfigOptions options,
        string key) => options.TryGetValue(key, out var stringValue) ? stringValue : null;

    internal static bool IsChildFile(string fileToCheck,
        IEnumerable<string> availableFiles,
        [NotNullWhen(true)] out CultureInfo? cultureInfo)
    {
        SplitName(fileToCheck, out var parentFileName, out var languageExtension);
        if (!availableFiles.Contains(parentFileName))
        {
            cultureInfo = null;
            return false;
        }
        var lastNumberOfCodes = 0;
        var sections = 0;
        foreach (var character in languageExtension)
        {
            switch (character)
            {
                case '-' when lastNumberOfCodes < 2 || sections > 1:
                    cultureInfo = null;
                    return false;
                case '-':
                    lastNumberOfCodes = 0;
                    sections++;
                    continue;
                default:
                    lastNumberOfCodes++;
                    break;
            }
        }
        if (lastNumberOfCodes is > 4 or < 2)
        {
            cultureInfo = null;
            return false;
        }

        try
        {
            cultureInfo = CultureInfo.GetCultureInfo(languageExtension);
        }
        catch (CultureNotFoundException)
        {
            cultureInfo = null;
            return false;
        }
        return true;
    }

    public static string GetIdentifierFromResourceName(string name)
    {
        if (name.All(CharExtensions.IsIdentifierPartCharacter))
        {
            return name[0].IsIdentifierStartCharacter() ? name : "_" + name;
        }

        var builder = new StringBuilder(name.Length);

        var f = name[0];
        if (f.IsIdentifierPartCharacter() && !f.IsIdentifierStartCharacter())
        {
            builder.Append('_');
        }

        foreach (var c in name)
        {
            builder.Append(c.IsIdentifierPartCharacter() ? c : '_');
        }

        return builder.ToString();
    }

    private readonly struct ResourceString
    {
        private static readonly Regex NamedParameterMatcher = new(@"\{([a-z]\w*)\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex NumberParameterMatcher = new(@"\{(\d+)\}", RegexOptions.Compiled);
        private readonly IReadOnlyList<string> _arguments;

        public ResourceString(string identifier, string value)
        {
            Identifier = identifier;
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

        public string Identifier { get; }
        public string Value { get; }

        public bool UsingNamedArgs { get; }

        public bool HasArguments => _arguments.Count > 0;

        public string GetArgumentNames() => string.Join(", ", _arguments.Select(a => "\"" + a + "\""));

        public IEnumerable<string> GetArguments()
        {
            var usingNamedArgs = UsingNamedArgs;
            return _arguments.Select(s => GetArgName(s, usingNamedArgs));
        }

        public string GetJoinedArguments() => string.Join(", ", GetArguments());

        public string GetMethodParameters()
        {
            var usingNamedArgs = UsingNamedArgs;
            return string.Join(", ", _arguments.Select(a => "object? " + GetArgName(a, usingNamedArgs)));
        }

        private static string GetArgName(string name, bool usingNamedArgs) => usingNamedArgs ? name : 'p' + name;
    }
}
