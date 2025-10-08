using System.Text;
using System.Web;

namespace CiteUrl.Core.Tokens;

/// <summary>
/// Builds strings (URLs, citation names) from token dictionaries using template parts.
/// </summary>
public class StringBuilder
{
    /// <summary>
    /// Ordered list of string parts with {token} placeholders.
    /// Parts are concatenated if all referenced tokens are available.
    /// </summary>
    public IReadOnlyList<string> Parts { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Operations to apply to tokens before building the string.
    /// Can create derived tokens via Output property.
    /// </summary>
    public IReadOnlyList<TokenOperation> Edits { get; init; } = Array.Empty<TokenOperation>();

    /// <summary>
    /// Default token values from template metadata.
    /// Merged with provided tokens (provided values take precedence).
    /// </summary>
    public IReadOnlyDictionary<string, string> Defaults { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// If true, applies URL encoding to the final result.
    /// </summary>
    public bool UrlEncode { get; init; } = true;

    /// <summary>
    /// Builds a string by applying edits and concatenating parts with token values.
    /// </summary>
    /// <param name="tokens">Token dictionary with values to substitute.</param>
    /// <param name="regexTimeout">Optional regex timeout for edit operations.</param>
    /// <returns>The constructed string, or null if all parts were skipped.</returns>
    public string? Build(Dictionary<string, string> tokens, TimeSpan? regexTimeout = null)
    {
        // Step 1: Merge defaults with provided tokens
        var mergedTokens = new Dictionary<string, string>(Defaults);
        foreach (var (key, value) in tokens)
        {
            mergedTokens[key] = value;
        }

        // Step 2: Execute edits (may modify tokens or create new ones)
        foreach (var edit in Edits)
        {
            if (edit.Token != null && mergedTokens.ContainsKey(edit.Token))
            {
                var transformedValue = edit.Apply(mergedTokens[edit.Token], regexTimeout);

                // Store in output token if specified, otherwise update original
                var targetToken = edit.Output ?? edit.Token;
                mergedTokens[targetToken] = transformedValue;
            }
        }

        // Step 3: Build string parts
        var builtParts = new List<string>();
        foreach (var part in Parts)
        {
            try
            {
                var builtPart = SubstituteTokens(part, mergedTokens);
                builtParts.Add(builtPart);
            }
            catch (KeyNotFoundException)
            {
                // Part references missing token - skip this part
                continue;
            }
        }

        // Step 4: Concatenate and encode
        if (builtParts.Count == 0)
            return null;

        var result = string.Concat(builtParts);

        if (UrlEncode)
        {
            // Simple URL encoding for common characters
            result = result.Replace(" ", "%20")
                          .Replace("(", "%28")
                          .Replace(")", "%29");
        }

        return result;
    }

    /// <summary>
    /// Substitutes {token} placeholders in a string with values from the dictionary.
    /// </summary>
    /// <param name="template">String with {token} placeholders.</param>
    /// <param name="tokens">Token dictionary.</param>
    /// <returns>String with placeholders replaced.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if a referenced token is not in the dictionary.</exception>
    private string SubstituteTokens(string template, Dictionary<string, string> tokens)
    {
        var result = new System.Text.StringBuilder(template);

        // Find and replace all {token} patterns
        var regex = new System.Text.RegularExpressions.Regex(@"\{([^}]+)\}");
        var matches = regex.Matches(template);

        // Process matches in reverse order to maintain string positions
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            var tokenName = match.Groups[1].Value;

            if (!tokens.ContainsKey(tokenName))
            {
                throw new KeyNotFoundException($"Token '{tokenName}' not found");
            }

            result.Remove(match.Index, match.Length);
            result.Insert(match.Index, tokens[tokenName]);
        }

        return result.ToString();
    }
}
