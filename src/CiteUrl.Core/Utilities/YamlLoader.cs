using System.Collections.Immutable;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CiteUrl.Core.Templates;
using CiteUrl.Core.Tokens;
using CiteUrl.Core.Exceptions;
using Serilog;

namespace CiteUrl.Core.Utilities;

/// <summary>
/// Loads templates from YAML content.
/// </summary>
public static class YamlLoader
{
    private static readonly ILogger? Logger = Log.Logger;

    /// <summary>
    /// Loads templates from YAML content string.
    /// </summary>
    /// <param name="yamlContent">YAML content to parse.</param>
    /// <param name="fileName">Optional filename for error reporting.</param>
    /// <returns>Dictionary of template name to Template object.</returns>
    /// <exception cref="CiteUrlYamlException">Thrown when YAML parsing fails.</exception>
    public static Dictionary<string, Template> LoadYaml(string yamlContent, string? fileName = null)
    {
        try
        {
            // Deserialize YAML to intermediate models
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build();

            var yamlTemplates = deserializer.Deserialize<Dictionary<string, TemplateYaml>>(yamlContent);

            if (yamlTemplates == null)
            {
                throw new CiteUrlYamlException("YAML deserialization returned null", fileName);
            }

            // Convert to Template objects with inheritance resolution
            return ConvertTemplates(yamlTemplates, fileName);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            Logger?.Error(ex, "YAML parsing failed for {FileName}", fileName ?? "<unknown>");
            throw new CiteUrlYamlException(
                $"YAML parsing error: {ex.Message}",
                fileName,
                (int)ex.Start.Line);
        }
        catch (CiteUrlYamlException)
        {
            throw; // Re-throw our custom exceptions
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Unexpected error loading YAML from {FileName}", fileName ?? "<unknown>");
            throw new CiteUrlYamlException(
                $"Error loading templates: {ex.Message}",
                fileName);
        }
    }

    private static Dictionary<string, Template> ConvertTemplates(
        Dictionary<string, TemplateYaml> yamlTemplates,
        string? fileName)
    {
        var templates = new Dictionary<string, Template>();
        var processed = new HashSet<string>();

        // Process templates in dependency order (resolve inheritance)
        foreach (var (name, _) in yamlTemplates)
        {
            ProcessTemplate(name, yamlTemplates, templates, processed, fileName);
        }

        return templates;
    }

    private static void ProcessTemplate(
        string name,
        Dictionary<string, TemplateYaml> yamlTemplates,
        Dictionary<string, Template> templates,
        HashSet<string> processed,
        string? fileName)
    {
        // Already processed
        if (processed.Contains(name))
            return;

        var yaml = yamlTemplates[name];

        // Process parent first if inheritance specified
        Template? parent = null;
        if (yaml.Inherit != null)
        {
            if (!yamlTemplates.ContainsKey(yaml.Inherit))
            {
                throw new CiteUrlYamlException(
                    $"Template '{name}' inherits from unknown template '{yaml.Inherit}'",
                    fileName);
            }

            ProcessTemplate(yaml.Inherit, yamlTemplates, templates, processed, fileName);
            parent = templates[yaml.Inherit];
        }

        // Convert YAML to Template
        try
        {
            var template = ConvertTemplate(name, yaml, parent, fileName);
            templates[name] = template;
            processed.Add(name);

            Logger?.Information("Loaded template: {TemplateName}", name);
        }
        catch (Exception ex) when (ex is not CiteUrlYamlException)
        {
            throw new CiteUrlYamlException(
                $"Error converting template '{name}': {ex.Message}",
                fileName);
        }
    }

    private static Template ConvertTemplate(
        string name,
        TemplateYaml yaml,
        Template? parent,
        string? fileName)
    {
        // Convert tokens
        var tokens = ImmutableDictionary<string, TokenType>.Empty;
        if (yaml.Tokens != null)
        {
            foreach (var (tokenName, tokenYaml) in yaml.Tokens)
            {
                tokens = tokens.Add(tokenName, ConvertTokenType(tokenYaml));
            }
        }

        // Convert metadata
        var metadata = yaml.Meta != null
            ? yaml.Meta.ToImmutableDictionary()
            : ImmutableDictionary<string, string>.Empty;

        // Convert builders
        var urlBuilder = yaml.UrlBuilder != null
            ? ConvertStringBuilder(yaml.UrlBuilder, isUrl: true)
            : null;

        var nameBuilder = yaml.NameBuilder != null
            ? ConvertStringBuilder(yaml.NameBuilder, isUrl: false)
            : null;

        // If parent exists, use inheritance
        if (parent != null)
        {
            return Template.Inherit(
                parent,
                name: name,
                tokens: tokens.Count > 0 ? tokens : null,
                metadata: metadata.Count > 0 ? metadata : null,
                patterns: yaml.GetPatterns(),
                broadPatterns: yaml.GetBroadPatterns(),
                shortformPatterns: yaml.GetShortformPatterns(),
                idformPatterns: yaml.GetIdformPatterns(),
                urlBuilder: urlBuilder,
                nameBuilder: nameBuilder
            );
        }

        // Create new template
        return new Template(
            name: name,
            tokens: tokens,
            metadata: metadata,
            patterns: yaml.GetPatterns(),
            broadPatterns: yaml.GetBroadPatterns(),
            shortformPatterns: yaml.GetShortformPatterns(),
            idformPatterns: yaml.GetIdformPatterns(),
            urlBuilder: urlBuilder,
            nameBuilder: nameBuilder
        );
    }

    private static TokenType ConvertTokenType(TokenTypeYaml yaml)
    {
        var edits = new List<TokenOperation>();
        foreach (var editObj in yaml.GetEdits())
        {
            edits.Add(ConvertTokenOperation(editObj));
        }

        return new TokenType
        {
            Regex = yaml.Regex ?? string.Empty,
            Edits = edits,
            Default = yaml.Default,
            IsSeverable = yaml.Severable ?? false
        };
    }

    private static TokenOperation ConvertTokenOperation(object editObj)
    {
        // Edit can be a dictionary with action keys
        if (editObj is Dictionary<object, object> dict)
        {
            var strDict = dict.ToDictionary(kv => kv.Key.ToString()!, kv => kv.Value);

            if (strDict.ContainsKey("sub"))
            {
                var subData = (List<object>)strDict["sub"];
                return new TokenOperation
                {
                    Action = TokenOperationAction.Sub,
                    Data = (subData[0].ToString()!, subData[1].ToString()!),
                    Token = strDict.GetValueOrDefault("token")?.ToString(),
                    Output = strDict.GetValueOrDefault("output")?.ToString(),
                    IsMandatory = strDict.GetValueOrDefault("mandatory") as bool? ?? true
                };
            }

            if (strDict.ContainsKey("lookup"))
            {
                var lookupDict = ((Dictionary<object, object>)strDict["lookup"])
                    .ToDictionary(kv => kv.Key.ToString()!, kv => kv.Value.ToString()!);

                return new TokenOperation
                {
                    Action = TokenOperationAction.Lookup,
                    Data = lookupDict,
                    Token = strDict.GetValueOrDefault("token")?.ToString(),
                    IsMandatory = strDict.GetValueOrDefault("mandatory") as bool? ?? true
                };
            }

            if (strDict.ContainsKey("case"))
            {
                return new TokenOperation
                {
                    Action = TokenOperationAction.Case,
                    Data = strDict["case"].ToString()!,
                    Token = strDict.GetValueOrDefault("token")?.ToString()
                };
            }

            if (strDict.ContainsKey("lpad"))
            {
                var lpadData = (List<object>)strDict["lpad"];
                return new TokenOperation
                {
                    Action = TokenOperationAction.LeftPad,
                    Data = (lpadData[0].ToString()![0], Convert.ToInt32(lpadData[1])),
                    Token = strDict.GetValueOrDefault("token")?.ToString()
                };
            }

            if (strDict.ContainsKey("number_style"))
            {
                var nsData = (List<object>)strDict["number_style"];
                return new TokenOperation
                {
                    Action = TokenOperationAction.NumberStyle,
                    Data = (nsData[0].ToString()!, nsData[1].ToString()!),
                    Token = strDict.GetValueOrDefault("token")?.ToString()
                };
            }
        }

        throw new InvalidOperationException($"Unknown edit operation format: {editObj}");
    }

    private static CiteUrl.Core.Tokens.StringBuilder ConvertStringBuilder(
        StringBuilderYaml yaml,
        bool isUrl)
    {
        var edits = new List<TokenOperation>();
        foreach (var editObj in yaml.GetEdits())
        {
            edits.Add(ConvertTokenOperation(editObj));
        }

        return new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = yaml.GetParts(),
            Edits = edits,
            UrlEncode = isUrl
        };
    }
}
