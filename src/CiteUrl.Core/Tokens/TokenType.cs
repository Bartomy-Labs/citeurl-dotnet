namespace CiteUrl.Core.Tokens;

/// <summary>
/// Defines a token that can be captured from text and normalized.
/// Immutable record representing token metadata and transformation pipeline.
/// </summary>
public record TokenType
{
    /// <summary>
    /// The regex pattern used to capture this token value.
    /// </summary>
    public string Regex { get; init; } = string.Empty;

    /// <summary>
    /// Ordered list of transformation operations to apply to captured values.
    /// </summary>
    public IReadOnlyList<TokenOperation> Edits { get; init; } = Array.Empty<TokenOperation>();

    /// <summary>
    /// Default value if token is not captured.
    /// </summary>
    public string? Default { get; init; }

    /// <summary>
    /// If true, this token can be truncated for authority matching.
    /// Used for hierarchical tokens like subsections.
    /// </summary>
    public bool IsSeverable { get; init; }

    /// <summary>
    /// Normalizes a raw captured token value by applying the edit pipeline.
    /// </summary>
    /// <param name="rawValue">The raw value captured by regex, or null.</param>
    /// <param name="regexTimeout">Optional regex timeout for edit operations.</param>
    /// <returns>The normalized value, or Default if rawValue is null.</returns>
    public string? Normalize(string? rawValue, TimeSpan? regexTimeout = null)
    {
        if (rawValue == null)
            return Default;

        var result = rawValue;
        foreach (var edit in Edits)
        {
            result = edit.Apply(result, regexTimeout);
        }

        return result;
    }
}
