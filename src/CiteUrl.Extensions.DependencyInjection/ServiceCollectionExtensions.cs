using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CiteUrl.Core.Templates;

namespace CiteUrl.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring CiteUrl services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CiteUrl citation parsing services to the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Optional configuration action for <see cref="CiteUrlOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so additional calls can be chained.</returns>
    /// <example>
    /// <code>
    /// // Basic registration with defaults
    /// services.AddCiteUrl();
    ///
    /// // With custom configuration
    /// services.AddCiteUrl(options =>
    /// {
    ///     options.RegexTimeout = TimeSpan.FromSeconds(2);
    ///     options.CustomYamlPaths = new[] { "custom-templates.yaml" };
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddCiteUrl(
        this IServiceCollection services,
        Action<CiteUrlOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<ICitator>(sp =>
        {
            var options = sp.GetService<IOptions<CiteUrlOptions>>()?.Value
                ?? new CiteUrlOptions();

            // For now, always use default templates
            // Future enhancement: Support custom YAML paths
            if (options.UseDefaultTemplates)
            {
                return Citator.Default;
            }

            // If custom YAML loading is needed in the future, implement here
            // For v1.0.0, we'll use the default templates
            return Citator.Default;
        });

        return services;
    }
}
