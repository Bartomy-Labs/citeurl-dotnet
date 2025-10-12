namespace CiteUrl.Extensions.DependencyInjection;

/// <summary>
/// Configuration options for CiteUrl services.
/// </summary>
public class CiteUrlOptions
{
    /// <summary>
    /// Gets or sets whether to use the default embedded YAML templates.
    /// Default is <c>true</c>.
    /// </summary>
    public bool UseDefaultTemplates { get; set; } = true;

    /// <summary>
    /// Gets or sets the regex timeout for citation pattern matching.
    /// Default is 1 second for ReDoS protection.
    /// </summary>
    public TimeSpan RegexTimeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets custom YAML file paths to load additional citation templates.
    /// When provided, these templates are loaded in addition to (or instead of) default templates
    /// depending on the <see cref="UseDefaultTemplates"/> setting.
    /// </summary>
    public string[]? CustomYamlPaths { get; set; }
}
