using YamlDotNet.Serialization;

namespace CiteUrl.Core.Utilities;

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
    public string? Pattern { get; set; }

    [YamlMember(Alias = "patterns")]
    public List<string>? Patterns { get; set; }

    [YamlMember(Alias = "broad pattern")]
    public string? BroadPattern { get; set; }

    [YamlMember(Alias = "broad patterns")]
    public List<string>? BroadPatterns { get; set; }

    [YamlMember(Alias = "shortform pattern")]
    public string? ShortformPattern { get; set; }

    [YamlMember(Alias = "shortform patterns")]
    public List<string>? ShortformPatterns { get; set; }

    [YamlMember(Alias = "idform pattern")]
    public string? IdformPattern { get; set; }

    [YamlMember(Alias = "idform patterns")]
    public List<string>? IdformPatterns { get; set; }

    [YamlMember(Alias = "URL builder")]
    public StringBuilderYaml? UrlBuilder { get; set; }

    [YamlMember(Alias = "name builder")]
    public StringBuilderYaml? NameBuilder { get; set; }

    [YamlMember(Alias = "inherit")]
    public string? Inherit { get; set; }

    /// <summary>
    /// Combines singular and plural pattern forms.
    /// </summary>
    public List<string> GetPatterns()
    {
        var result = new List<string>();
        if (Pattern != null) result.Add(Pattern);
        if (Patterns != null) result.AddRange(Patterns);
        return result;
    }

    public List<string> GetBroadPatterns()
    {
        var result = new List<string>();
        if (BroadPattern != null) result.Add(BroadPattern);
        if (BroadPatterns != null) result.AddRange(BroadPatterns);
        return result;
    }

    public List<string> GetShortformPatterns()
    {
        var result = new List<string>();
        if (ShortformPattern != null) result.Add(ShortformPattern);
        if (ShortformPatterns != null) result.AddRange(ShortformPatterns);
        return result;
    }

    public List<string> GetIdformPatterns()
    {
        var result = new List<string>();
        if (IdformPattern != null) result.Add(IdformPattern);
        if (IdformPatterns != null) result.AddRange(IdformPatterns);
        return result;
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
    public string? Part { get; set; }

    [YamlMember(Alias = "parts")]
    public List<string>? Parts { get; set; }

    [YamlMember(Alias = "edit")]
    public object? Edit { get; set; }

    [YamlMember(Alias = "edits")]
    public List<object>? Edits { get; set; }

    public List<string> GetParts()
    {
        var result = new List<string>();
        if (Part != null) result.Add(Part);
        if (Parts != null) result.AddRange(Parts);
        return result;
    }

    public List<object> GetEdits()
    {
        var result = new List<object>();
        if (Edit != null) result.Add(Edit);
        if (Edits != null) result.AddRange(Edits);
        return result;
    }
}
