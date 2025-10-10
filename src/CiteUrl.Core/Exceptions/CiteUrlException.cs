namespace CiteUrl.Core.Exceptions;

/// <summary>
/// Base exception for CiteUrl library errors.
/// </summary>
public class CiteUrlException : Exception
{
    public CiteUrlException(string message) : base(message) { }
    public CiteUrlException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when YAML parsing or deserialization fails.
/// </summary>
public class CiteUrlYamlException : CiteUrlException
{
    public string? YamlFileName { get; init; }
    public int? LineNumber { get; init; }

    public CiteUrlYamlException(string message) : base(message) { }

    public CiteUrlYamlException(string message, Exception innerException)
        : base(message, innerException) { }

    public CiteUrlYamlException(string message, string? yamlFileName, int? lineNumber = null)
        : base(message)
    {
        YamlFileName = yamlFileName;
        LineNumber = lineNumber;
    }
}

/// <summary>
/// Exception thrown when regex compilation fails.
/// </summary>
public class CiteUrlRegexException : CiteUrlException
{
    public string? TemplateName { get; init; }
    public string? Pattern { get; init; }

    public CiteUrlRegexException(string message) : base(message) { }

    public CiteUrlRegexException(string message, Exception innerException)
        : base(message, innerException) { }

    public CiteUrlRegexException(string message, string? templateName, string? pattern = null)
        : base(message)
    {
        TemplateName = templateName;
        Pattern = pattern;
    }
}

/// <summary>
/// Exception thrown when token normalization fails.
/// </summary>
public class CiteUrlTokenException : CiteUrlException
{
    public string? TokenName { get; init; }
    public string? InputValue { get; init; }

    public CiteUrlTokenException(string message) : base(message) { }

    public CiteUrlTokenException(string message, Exception innerException)
        : base(message, innerException) { }

    public CiteUrlTokenException(string message, string? tokenName, string? inputValue = null)
        : base(message)
    {
        TokenName = tokenName;
        InputValue = inputValue;
    }
}
