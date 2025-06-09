using Microsoft.EntityFrameworkCore;

using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Web;

using Npgsql;

using OpenTelemetry.Metrics;

namespace PowerUnit;

internal sealed class Program
{
    private const string SERVICE_NAME = "PowerUnitIEC104ExportService";

    private static async Task Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        while (true)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseDefaultServiceProvider((context, options) => { options.ValidateScopes = true; })
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                    })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    services.Configure<HostOptions>(hostOptions =>
                    {
                        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
                    });

                    services.AddSingleton(TimeProvider.System);

                    services.AddEnviromentManager(SERVICE_NAME);

                    services.AddPowerUnitDbContext(hostBuilderContext.Configuration);

                    // внешний
                    services.AddScoped<IDataProvider, DataProvider>();
                    services.AddSingleton<IConfigProvider, ConfigProvider>();
                    services.AddTransient<IFileProvider, FileProvider>();

                    // внутренний
                    services.AddSingleton<IEC104ServerFactory>();
                    services.AddOptions<IEC104ServersOptions>().Bind(hostBuilderContext.Configuration.GetSection(nameof(IEC104ServersOptions)));
                    services.AddTransient<IEC104Server>();
                    services.AddTimeoutService();

                    services.AddSingleton(s =>
                    {
                        var iecReflection = new IecParserGenerator([]);
                        iecReflection.Validate();
                        return iecReflection;
                    });

                    services.AddOpenTelemetry().WithMetrics(pb =>
                    {
                        pb
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddNpgsqlInstrumentation()
                        .AddPrometheusExporter(opt =>
                        {
                            opt.ScrapeEndpointPath = "/metrics";
                        })
                        .AddOtlpExporter((opt, readerOpt) =>
                        {
                            opt.Endpoint = new Uri("http://host.docker.internal:4317");
                            readerOpt.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
                        });
                    });

                    services.AddHostedService<IEC104ServersStarterService>();
                })
                .ConfigureLogging((hostBuilderContext, logging) =>
                {
                    logging.ClearProviders();
                    LogManager.Configuration = new NLogLoggingConfiguration(hostBuilderContext.Configuration.GetSection("NLog"));
                    var fileTargetConfig = LogManager.Configuration.FindTargetByName<AsyncTargetWrapper>("logfile");
                    var enviromentManager = EnviromentManagerDiExtension.GetEnviromentManager(SERVICE_NAME);
                    if (fileTargetConfig.WrappedTarget is FileTarget fileTarget)
                    {
                        fileTarget.FileName = Path.Combine(enviromentManager.GetLogPath(), "current.log");
                        fileTarget.ArchiveFileName = Path.Combine(enviromentManager.GetLogPath(), "{#}.log");
                    }
                })
                .UseNLog();

                var host = builder.Build();

                host.UseOpenTelemetryPrometheusScrapingEndpoint();

                using (var scope = host.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<PowerUnitDbContext>();
                    await db.Database.MigrateAsync();
                }

                await host.RunAsync();
                break;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error start service, retry after 5s");
                await Task.Delay(5_000);
                continue;
            }
        }

        logger.Info("Shutdown service");
        LogManager.Shutdown();
    }
}
