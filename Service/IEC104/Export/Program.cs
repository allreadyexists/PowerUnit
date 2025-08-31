using Microsoft.EntityFrameworkCore;

using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Web;

using OpenTelemetry.Metrics;

using PowerUnit.Common.EnviromentManager;
using PowerUnit.Common.Subsciption;
using PowerUnit.Common.TimeoutService;
using PowerUnit.Infrastructure.IEC104ServerDb;
using PowerUnit.Infrastructure.IEC104ServerDb.PostgreSql;
using PowerUnit.Infrastructure.IEC104ServerDb.Sqlite;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Export.DataSource;
using PowerUnit.Service.IEC104.Types;

using System.Reflection;

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

                    var dbProvider = hostBuilderContext.Configuration.GetValue("DbProvider", "Sqlite");
                    switch (dbProvider)
                    {
                        case "Sqlite":
                            services.AddPowerUnitIEC104ServerDbContextSqlite(hostBuilderContext.Configuration);
                            break;
                        case "PostgreSql":
                            services.AddPowerUnitIEC104ServerPostgreSqlDbContext(hostBuilderContext.Configuration);
                            break;
                        default:
                            throw new Exception($"Unsupported provider: {dbProvider}");
                    }

                    // внешний
                    services.AddSingleton<IDataSource<BaseValue>, BaseValueTestDataSource>();
                    services.AddSingleton<IConfigProvider, IEC104ConfigProvider>();

                    // внутренний
                    services.AddSingleton<IIEC60870_5_104ChannelLayerDiagnostic, IEC104ChannelLayerDiagnostic>();
                    services.AddSingleton<IEC104ServerFactory>();
                    services.AddTimeoutService(ServiceLifetime.Transient);

                    services.AddSingleton(s =>
                    {
                        var iecReflection = ActivatorUtilities.CreateInstance<IECParserGenerator>(s, [Array.Empty<Assembly>()]);
                        iecReflection.Validate();
                        return iecReflection;
                    });

                    services.AddOpenTelemetry().WithMetrics(pb =>
                    {
                        pb
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddMeter(IEC104ChannelLayerDiagnostic.MeterName)
                        .AddEventCountersInstrumentation(c =>
                            c.AddEventSources("System.Net.Sockets"))
                        .AddPrometheusExporter(opt =>
                        {
                            opt.ScrapeEndpointPath = "/metrics";
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
                    if (fileTargetConfig!.WrappedTarget is FileTarget fileTarget)
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
                    using var db = scope.ServiceProvider.GetRequiredService<IPowerUnitIEC104ServerDbContext>();
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
