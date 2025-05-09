using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Prometheus;
using Prometheus.DotNetRuntime;

namespace K2.Common.Prometheus;

/// <summary>
/// Prometheus .NET metrics service
/// </summary>
public class PrometheusMetricsService : BackgroundService
{
    private readonly MetricHandler _metricServer;
    private readonly PrometheusConfig _config;

    private readonly ILogger<PrometheusMetricsService> _logger;

    private const int RESET_HOURS = 3;

    public PrometheusMetricsService(
        IOptions<PrometheusConfig> options,
        ILogger<PrometheusMetricsService> logger)
    {
        _logger = logger;
        _config = options.Value;

        if (_config.UseKestrel)
            _metricServer = new KestrelMetricServer(_config.Host, _config.Port);
        else
            _metricServer = new MetricServer(_config.Host, _config.Port);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Starting prometheus server on {Host}:{Port}", _config.Host, _config.Port);

        _metricServer.Start();

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //working cycle
        //this cycle periodically dispose of metrics collector to ensure resource usage
        //main problem was with now removed metrics setup WithThreadPoolSchedulingStats(), but lest dispose collector every few hours
        //see https://github.com/djluck/prometheus-net.DotNetRuntime/issues/6

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Restarting prometheus metrics collection");
            var builder = DotNetRuntimeStatsBuilder.Customize()
#if DEBUG
                 .WithDebuggingMetrics(true)
#endif
            ;

            using var collector = builder.StartCollecting();
            await Task.Delay(TimeSpan.FromHours(RESET_HOURS), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping prometheus server");
        await _metricServer.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}
