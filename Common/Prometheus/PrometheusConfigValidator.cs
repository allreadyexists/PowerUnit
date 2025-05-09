using Microsoft.Extensions.Options;

namespace K2.Common.Prometheus;

public class PrometheusConfigValidator : IValidateOptions<PrometheusConfig>
{
    public ValidateOptionsResult Validate(string name, PrometheusConfig options)
    {
        if (string.IsNullOrEmpty(options.Host))
            return ValidateOptionsResult.Fail("Prometheus metric server host is not configured");

        if (options.Port <= 0)
            return ValidateOptionsResult.Fail("Prometheus metric server port is not configured");

        return ValidateOptionsResult.Success;
    }
}
