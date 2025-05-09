using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace K2.Common.Prometheus;

public static class PrometheusDiExtension
{
    /// <summary>
    /// Add .NET statistics collection for Prometheus
    /// </summary>
    public static void AddPrometheusDotNetMetrics(this IServiceCollection services, IConfiguration config)
    {
        services.AddMetrics(config.GetSection(PrometheusConfig.CONFIG_SECTION_NAME));
    }

    /// <summary>
    /// Add .NET statistics collection for Prometheus
    /// </summary>
    public static void AddPrometheusDotNetMetrics(this IServiceCollection services, Func<IConfigurationSection> config) => services.AddMetrics(config());

    private static void AddMetrics(this IServiceCollection services, IConfigurationSection prometheusConfigSection)
    {
        services.Configure<PrometheusConfig>(prometheusConfigSection);
        services.AddSingleton<IValidateOptions<PrometheusConfig>, PrometheusConfigValidator>();
        services.AddHostedService<PrometheusMetricsService>();
    }
}
