using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace CiteUrl.Core.Utilities;

/// <summary>
/// Custom YAML converter that handles tokens defined as simple strings or as TokenTypeYaml objects.
/// Example: "volume: \d+" becomes TokenTypeYaml { Regex = "\d+" }
/// </summary>
public class TokenTypeYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(TokenTypeYaml);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        // Try to read as scalar (simple string regex)
        if (parser.Current is Scalar scalar)
        {
            var regexValue = scalar.Value;
            parser.MoveNext();
            return new TokenTypeYaml { Regex = regexValue };
        }

        // Otherwise manually deserialize the mapping
        if (parser.TryConsume<MappingStart>(out _))
        {
            var result = new TokenTypeYaml();

            while (!parser.TryConsume<MappingEnd>(out _))
            {
                var key = parser.Consume<Scalar>();
                var keyName = key.Value;

                if (keyName == "regex")
                {
                    var value = parser.Consume<Scalar>();
                    result.Regex = value.Value;
                }
                else if (keyName == "default")
                {
                    var value = parser.Consume<Scalar>();
                    result.Default = value.Value;
                }
                else if (keyName == "severable")
                {
                    var value = parser.Consume<Scalar>();
                    result.Severable = ParseYamlBoolean(value.Value);
                }
                else if (keyName == "edit")
                {
                    // Use rootDeserializer for complex objects, but not for TokenTypeYaml
                    result.Edit = rootDeserializer(typeof(object));
                }
                else if (keyName == "edits")
                {
                    result.Edits = (List<object>)rootDeserializer(typeof(List<object>));
                }
                else
                {
                    // Skip unknown keys
                    parser.SkipThisAndNestedEvents();
                }
            }

            return result;
        }

        throw new InvalidOperationException($"Unexpected YAML structure for TokenTypeYaml");
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        // We don't need to implement serialization for this use case
        throw new NotImplementedException("TokenTypeYaml serialization is not implemented");
    }

    /// <summary>
    /// Parses YAML boolean values (yes/no/true/false/on/off) to .NET boolean
    /// </summary>
    private static bool ParseYamlBoolean(string value)
    {
        var normalized = value.ToLowerInvariant();
        return normalized switch
        {
            "yes" or "true" or "on" => true,
            "no" or "false" or "off" => false,
            _ => throw new ArgumentException($"Cannot parse '{value}' as boolean")
        };
    }
}

/// <summary>
/// YAML deserialization model for templates.
/// All properties nullable to support template inheritance.
/// </summary>
public class TemplateYaml
{
    [YamlMember(Alias = "tokens")]
    public Dictionary<string, TokenTypeYaml>? Tokens { get; set; }

    [YamlMember(Alias = "meta")]
    public Dictionary<string, string>? Meta { get; set; }

    [YamlMember(Alias = "pattern")]
    public object? Pattern { get; set; }

    [YamlMember(Alias = "patterns")]
    public List<object>? Patterns { get; set; }

    [YamlMember(Alias = "broad pattern")]
    public object? BroadPattern { get; set; }

    [YamlMember(Alias = "broad patterns")]
    public List<object>? BroadPatterns { get; set; }

    [YamlMember(Alias = "shortform pattern")]
    public object? ShortformPattern { get; set; }

    [YamlMember(Alias = "shortform patterns")]
    public List<object>? ShortformPatterns { get; set; }

    [YamlMember(Alias = "idform pattern")]
    public object? IdformPattern { get; set; }

    [YamlMember(Alias = "idform patterns")]
    public List<object>? IdformPatterns { get; set; }

    [YamlMember(Alias = "URL builder")]
    public object? UrlBuilder { get; set; }

    [YamlMember(Alias = "name builder")]
    public object? NameBuilder { get; set; }

    [YamlMember(Alias = "inherit")]
    public string? Inherit { get; set; }

    /// <summary>
    /// Combines singular and plural pattern forms.
    /// Flattens nested lists (e.g., [[a, b], [c, d]] becomes ["ab", "cd"])
    /// </summary>
    public List<string> GetPatterns()
    {
        var result = new List<string>();
        if (Pattern != null) result.Add(FlattenPattern(Pattern));
        if (Patterns != null)
            result.AddRange(Patterns.Select(FlattenPattern));
        return result;
    }

    public List<string> GetBroadPatterns()
    {
        var result = new List<string>();
        if (BroadPattern != null) result.Add(FlattenPattern(BroadPattern));
        if (BroadPatterns != null)
            result.AddRange(BroadPatterns.Select(FlattenPattern));
        return result;
    }

    public List<string> GetShortformPatterns()
    {
        var result = new List<string>();
        if (ShortformPattern != null) result.Add(FlattenPattern(ShortformPattern));
        if (ShortformPatterns != null)
            result.AddRange(ShortformPatterns.Select(FlattenPattern));
        return result;
    }

    public List<string> GetIdformPatterns()
    {
        var result = new List<string>();
        if (IdformPattern != null) result.Add(FlattenPattern(IdformPattern));
        if (IdformPatterns != null)
            result.AddRange(IdformPatterns.Select(FlattenPattern));
        return result;
    }

    /// <summary>
    /// Flattens a pattern that may be a string or nested list into a single string.
    /// Examples:
    ///   "foo" -> "foo"
    ///   ["foo", "bar"] -> "foobar"
    ///   [["a", "b"], "c"] -> "abc" (recursively flattens)
    /// </summary>
    private static string FlattenPattern(object pattern)
    {
        if (pattern is string s)
            return s;

        if (pattern is List<object> list)
            return string.Concat(list.Select(FlattenPattern));

        // Handle cases where YAML parser returns IList or IEnumerable
        if (pattern is System.Collections.IEnumerable enumerable and not string)
        {
            var parts = new List<string>();
            foreach (var item in enumerable)
            {
                parts.Add(FlattenPattern(item));
            }
            return string.Concat(parts);
        }

        return pattern?.ToString() ?? string.Empty;
    }
}

public class TokenTypeYaml
{
    [YamlMember(Alias = "regex")]
    public string? Regex { get; set; }

    [YamlMember(Alias = "edit")]
    public object? Edit { get; set; }

    [YamlMember(Alias = "edits")]
    public List<object>? Edits { get; set; }

    [YamlMember(Alias = "default")]
    public string? Default { get; set; }

    [YamlMember(Alias = "severable")]
    public bool? Severable { get; set; }

    public List<object> GetEdits()
    {
        var result = new List<object>();
        if (Edit != null) result.Add(Edit);
        if (Edits != null) result.AddRange(Edits);
        return result;
    }
}

public class StringBuilderYaml
{
    [YamlMember(Alias = "part")]
    public object? Part { get; set; }

    [YamlMember(Alias = "parts")]
    public List<object>? Parts { get; set; }

    [YamlMember(Alias = "edit")]
    public object? Edit { get; set; }

    [YamlMember(Alias = "edits")]
    public List<object>? Edits { get; set; }

    public List<string> GetParts()
    {
        var result = new List<string>();
        if (Part != null) result.Add(ConvertToString(Part));
        if (Parts != null)
            result.AddRange(Parts.Select(ConvertToString));
        return result;
    }

    public List<object> GetEdits()
    {
        var result = new List<object>();
        if (Edit != null) result.Add(Edit);
        if (Edits != null) result.AddRange(Edits);
        return result;
    }

    private static string ConvertToString(object obj)
    {
        if (obj is string s)
            return s;
        return obj?.ToString() ?? string.Empty;
    }
}
