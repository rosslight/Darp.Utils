namespace Darp.Utils.ResxSourceGenerator;

using System.Diagnostics.CodeAnalysis;
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

    private void LogError(string message) => OutputText = SourceText.From($"#error {message}", Encoding.UTF8, SourceHashAlgorithm.Sha256);

    [MemberNotNullWhen(true, nameof(OutputTextHintName), nameof(OutputText))]
    public bool Execute(CancellationToken cancellationToken)
    {
        OutputTextHintName = $"{ResourceInformation.ResourceHintName}.Designer.cs";

        if (string.IsNullOrEmpty(ResourceInformation.ResourceName))
        {
            LogError("ResourceName not specified");
            return false;
        }

        var resourceClazzName = ResourceInformation.ResourceClassName;
        var resourceAccessName = resourceClazzName is null || string.IsNullOrEmpty(resourceClazzName)
            ? ResourceInformation.ResourceName
            : resourceClazzName;
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
        List< (string Identifier, string Name)> resourceNames = [];
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


            var propertyIdentifier = GetIdentifierFromResourceName(name);
            resourceNames.Add((propertyIdentifier, name));
            var trimmedValue = value.Length > MaxDocCommentLength ? value[..MaxDocCommentLength] + " ..." : value;

            var propertyString = $"""
{memberIndent}/// <summary>Get the resource of <see cref="Keys.@{propertyIdentifier}"/></summary>
{memberIndent}/// <value>{trimmedValue}</value>
{memberIndent}public string @{propertyIdentifier} => GetResourceString(Keys.@{propertyIdentifier});
""";
            strings.AppendLine(propertyString);
            if (!ResourceInformation.EmitFormatMethods) continue;
            var resourceString = new ResourceString(name, value);
            if (!resourceString.HasArguments) continue;
            RenderDocCommentSummary(memberIndent, strings, trimmedValue);
            RenderFormatMethod(memberIndent, strings, resourceString);
        }

        var getResourceStringAttributes = new List<string>();
        if (CompilationInformation.HasAggressiveInlining)
        {
            getResourceStringAttributes.Add("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        }

        var getStringMethod = $"""
{memberIndent}/// <summary>Get a resource of the <see cref="ResourceManager"/> with the configured <see cref="Culture"/> as a string</summary>
{memberIndent}/// <param name="resourceName">The name of the resource to get</param>
{memberIndent}/// <returns>Returns the resource value as a string or the <paramref name="resourceName"/> if it could not be found</returns>
{string.Join(Environment.NewLine, getResourceStringAttributes.Select(attr => memberIndent + attr))}
{memberIndent}public string GetResourceString(string resourceName) => ResourceManager.GetString(resourceName, Culture) ?? resourceName;
""";
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
        if (string.IsNullOrEmpty(ResourceInformation.ResourceClassName)
            || ResourceInformation.ResourceName == ResourceInformation.ResourceClassName)
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
            var hasNamespace = SplitName(resourceTypeName, out var resourceNamespaceName, out var resourceClassName);
            if (!hasNamespace || resourceNamespaceName is null)
            {
                LogError($"No namespace for resource type name '{resourceTypeName}' provided. That should not happen");
                return false;
            }
            resourceTypeDefinition = $$"""

namespace {{resourceNamespaceName}}
{
    internal static class {{resourceClassName}} { }
}
""";
        }

        var namesClass = $$"""
{{memberIndent}}/// <summary>All keys contained in <see cref="{{className}}"/></summary>
{{memberIndent}}public static class Keys
{{memberIndent}}{
{{string.Join(Environment.NewLine, resourceNames.Select(x => $"{memberIndent}    public const string @{x.Identifier} = @\"{x.Name}\";"))}}
{{memberIndent}}}
""";

        // The ResourceManager property being initialized lazily is an important optimization that lets .NETNative
        // completely remove the ResourceManager class if the disk space saving optimization to strip resources
        // (/DisableExceptionMessages) is turned on in the compiler.
        var result = $$"""
// <auto-generated/>

#nullable enable
using System.Reflection;
{{resourceTypeDefinition}}
{{namespaceStart}}
{{classIndent}}/// <summary>A strongly typed resource class for '{{ResourceInformation.ResourceFile.Path}}'</summary>
{{classIndent}}{{(ResourceInformation.Public ? "public" : "internal")}} sealed partial class {{className}}
{{classIndent}}{
{{memberIndent}}private static {{className}}? _default;
{{memberIndent}}/// <summary>The Default implementation of <see cref="{{className}}"/></summary>
{{memberIndent}}public static {{className}} Default => _default ??= new {{className}}();

{{memberIndent}}public delegate void CultureUpdateDelegate(global::System.Globalization.CultureInfo? oldCulture, global::System.Globalization.CultureInfo? newCulture);
{{memberIndent}}/// <summary>Called after the <see cref="Culture"/> was updated. Provides previous culture and the newly set culture</summary>
{{memberIndent}}public event CultureUpdateDelegate? CultureUpdated;

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
{{memberIndent}}            CultureUpdated?.Invoke(oldCulture, value);
{{memberIndent}}    }
{{memberIndent}}}

{{memberIndent}}///<summary>Returns the cached ResourceManager instance used by this class.</summary>
{{memberIndent}}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Advanced)]
{{memberIndent}}public global::System.Resources.ResourceManager ResourceManager { get; } = new global::System.Resources.ResourceManager(typeof({{resourceTypeName}}));

{{getStringMethod}}

{{strings}}
{{namesClass}}
{{classIndent}}}
{{namespaceEnd}}

""";
        OutputText = SourceText.From(result.Replace("\r\n", "\n"), Encoding.UTF8, SourceHashAlgorithm.Sha256);
        return true;
    }

    private static string GetIdentifierFromResourceName(string name)
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

    private static void RenderDocCommentSummary(string memberIndent, StringBuilder strings, string value) =>
        RenderDocComment(memberIndent, strings, "summary", value);
    private static void RenderDocComment(string memberIndent, StringBuilder strings, string element, string value)
    {
        var escapedTrimmedValue = new XElement(element, value).ToString();

        foreach (var line in escapedTrimmedValue.Split(Separator, StringSplitOptions.None))
        {
            strings.Append(memberIndent)
                .Append("///")
                .Append(' ')
                .AppendLine(line);
        }
    }

    private static bool SplitName(string fullName,
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

        public string GetMethodParameters() => string.Join(", ", _arguments.Select(a => "object? " + GetArgName(a)));

        private string GetArgName(string name) => UsingNamedArgs ? name : 'p' + name;
    }
}
