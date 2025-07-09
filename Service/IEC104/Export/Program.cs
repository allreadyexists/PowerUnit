using Microsoft.EntityFrameworkCore;

using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Web;

using Npgsql;

using OpenTelemetry.Metrics;

using PowerUnit.Common.EnviromentManager;
using PowerUnit.Common.Subsciption;
using PowerUnit.Infrastructure.IEC104ServerDb;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Options;
using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104.Export;

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

                    services.AddPowerUnitIEC104ServerDbContext(hostBuilderContext.Configuration);

                    // внешний
                    //services.AddScoped<IDataSource, TestDataSource>();
                    //services.AddScoped<IDataProvider, TestDataProvider>();
                    services.AddSingleton<IDataSource<AnalogValue>, AnalogValueTestDataSource>();
                    services.AddSingleton<IDataSource<DiscretValue>, DiscretValueTestDataSource>();
                    services.AddSingleton<IConfigProvider, ConfigProvider>();

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
                        .AddEventCountersInstrumentation(c =>
                            c.AddEventSources("System.Net.Sockets"))
                        .AddPrometheusExporter(opt =>
                        {
                            opt.ScrapeEndpointPath = "/metrics";
                        })
                        .AddOtlpExporter((opt, readerOpt) =>
                        {
                            //opt.Endpoint = new Uri("http://host.docker.internal:4317");
                            opt.Endpoint = new Uri("http://localhost:4317");
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
                    var db = scope.ServiceProvider.GetRequiredService<PowerUnitIEC104ServerDbContext>();
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
