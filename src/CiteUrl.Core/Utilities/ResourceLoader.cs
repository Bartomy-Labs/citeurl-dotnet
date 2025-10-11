using System.Reflection;
using CiteUrl.Core.Exceptions;
using Serilog;

namespace CiteUrl.Core.Utilities;

/// <summary>
/// Loads embedded YAML template resources from the assembly.
/// </summary>
public static class ResourceLoader
{
    private static readonly ILogger? Logger = Log.Logger;

    /// <summary>
    /// Gets the names of all embedded YAML template resources.
    /// </summary>
    public static string[] GetEmbeddedYamlResourceNames()
    {
        var assembly = typeof(ResourceLoader).Assembly;
        return assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    /// <summary>
    /// Loads an embedded YAML resource by name.
    /// </summary>
    /// <param name="resourceName">Simple name like "caselaw.yaml" or full resource name.</param>
    /// <returns>YAML content as string.</returns>
    /// <exception cref="CiteUrlYamlException">Thrown if resource not found.</exception>
    public static string LoadEmbeddedYaml(string resourceName)
    {
        var assembly = typeof(ResourceLoader).Assembly;

        // Try exact match first
        var fullResourceName = resourceName;

        // If not a full resource name, search for it
        if (!resourceName.Contains(".", StringComparison.Ordinal) ||
            !assembly.GetManifestResourceNames().Contains(resourceName))
        {
            var matching = assembly.GetManifestResourceNames()
                .Where(name => name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (matching.Length == 0)
            {
                throw new CiteUrlYamlException(
                    $"Embedded resource '{resourceName}' not found. " +
                    $"Available resources: {string.Join(", ", GetEmbeddedYamlResourceNames())}");
            }

            if (matching.Length > 1)
            {
                throw new CiteUrlYamlException(
                    $"Ambiguous resource name '{resourceName}'. Matches: {string.Join(", ", matching)}");
            }

            fullResourceName = matching[0];
        }

        using var stream = assembly.GetManifestResourceStream(fullResourceName);

        if (stream == null)
        {
            throw new CiteUrlYamlException($"Failed to load embedded resource: {fullResourceName}");
        }

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        Logger?.Debug("Loaded embedded YAML resource: {ResourceName} ({Bytes} bytes)",
            resourceName, content.Length);

        return content;
    }

    /// <summary>
    /// Loads all default embedded YAML templates.
    /// </summary>
    /// <returns>Combined YAML content from all 5 default files.</returns>
    public static string LoadAllDefaultYaml()
    {
        var defaultFiles = new[]
        {
            "caselaw.yaml",
            "general-federal-law.yaml",
            "specific-federal-laws.yaml",
            "state-law.yaml",
            "secondary-sources.yaml"
        };

        var combined = new System.Text.StringBuilder();

        foreach (var file in defaultFiles)
        {
            var content = LoadEmbeddedYaml(file);
            combined.AppendLine(content);
            combined.AppendLine(); // Separate YAML documents
        }

        Logger?.Information("Loaded {FileCount} default YAML template files", defaultFiles.Length);

        return combined.ToString();
    }
}
