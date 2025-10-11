using System.Collections.Immutable;
using CiteUrl.Core.Templates;
using CiteUrl.Core.Tokens;

namespace CiteUrl.Core.Models;

public record class Authority
{
    public Template Template { get; init; } = null!;
    public ImmutableDictionary<string, string> Tokens { get; init; } = ImmutableDictionary<string, string>.Empty;
    public List<Citation> Citations { get; init; } = new();
    public IReadOnlyList<string> IgnoredTokens { get; init; } = Array.Empty<string>();

    public string? Url => Template.UrlBuilder?.Build(Tokens.ToDictionary(k => k.Key, v => v.Value));
    public string? Name => Template.NameBuilder?.Build(Tokens.ToDictionary(k => k.Key, v => v.Value))
        ?? DeriveName();

    private string? DeriveName()
    {
        var first = Citations.FirstOrDefault(c => c.Parent == null);
        return first?.Name ?? first?.Text;
    }

    public bool Contains(Citation citation)
    {
        if (citation.Template.Name != Template.Name)
            return false;

        foreach (var (tokenName, tokenValue) in Tokens)
        {
            if (!citation.Tokens.TryGetValue(tokenName, out var citationValue))
                return false;

            var tokenType = Template.Tokens[tokenName];
            if (tokenType.IsSeverable)
            {
                if (!citationValue.StartsWith(tokenValue))
                    return false;
            }
            else if (citationValue != tokenValue)
            {
                return false;
            }
        }

        return true;
    }
}
