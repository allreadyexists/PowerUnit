using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

using PowerUnit.Common.TimeoutService;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Import;
using PowerUnit.Service.IEC104.Types;

using System.Reflection;

namespace PowerUnit.Service.IEC104.Export;

internal sealed class IEC60870_5_104ChannelLayerEmptyDiagnostic : IIEC60870_5_104ChannelLayerDiagnostic
{
    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgSend(string serverId, ChannelLayerPacketPriority priority) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgSkip(string serverId, ChannelLayerPacketPriority priority) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgTotal(string serverId, ChannelLayerPacketPriority priority) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.ProtocolError(string serverId) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.RcvIPacket(string serverId) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.RcvSPacket(string serverId) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.RcvUPacket(string serverId) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.SendIPacket(string serverId) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.SendSPacket(string serverId) { }
    void IIEC60870_5_104ChannelLayerDiagnostic.SendUPacket(string serverId) { }
}

internal sealed class Program
{
    //private static readonly string[] _uriPrefixes = ["http://localhost:5000/"];

    private static async Task Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        while (true)
        {
            try
            {
                var builder = Host.CreateDefaultBuilder(args);
                builder.UseDefaultServiceProvider((context, options) => { options.ValidateScopes = true; })
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                    })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    services.Configure<HostOptions>(hostOptions =>
                    {
                        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
                    });

                    //var serviceName = hostBuilderContext.Configuration.GetValue("ServiceName", "IEC104Export");

                    services.AddSingleton(TimeProvider.System);

                    //services.AddEnviromentManager(serviceName);

                    //var dbProvider = hostBuilderContext.Configuration.GetValue("DbProvider", "Sqlite");
                    //switch (dbProvider)
                    //{
                    //    case "Sqlite":
                    //        services.AddPowerUnitIEC104ServerDbContextSqlite(hostBuilderContext.Configuration);
                    //        break;
                    //    case "PostgreSql":
                    //        services.AddPowerUnitIEC104ServerPostgreSqlDbContext(hostBuilderContext.Configuration);
                    //        break;
                    //    default:
                    //        throw new Exception($"Unsupported db provider: {dbProvider}");
                    //}

                    //// внешний
                    //services.AddBaseValueTestDataSource(hostBuilderContext.Configuration);
                    //services.AddSingleton<IConfigProvider, IEC104ConfigProvider>();

                    // внутренний
                    //services.AddSingleton<IEC104ServerFactory>();
                    services.AddTimeoutService(ServiceLifetime.Transient);
                    //services.AddSingleton<IDataProvider, IEC104DataProvider>();

                    //services.AddSingleton<IEC104Diagnostic>();
                    //services.AddSingleton<IIEC60870_5_104ChannelLayerDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());
                    //services.AddSingleton<IIEC60870_5_104ApplicationLayerDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());
                    //services.AddSingleton<ITimeoutServiceDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());
                    //services.AddSingleton<ITestDataSourceDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());
                    //services.AddSingleton<ISubscriberDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());

                    services.AddSingleton(s =>
                    {
                        var iecReflection = ActivatorUtilities.CreateInstance<IECParserGenerator>(s, [Array.Empty<Assembly>()]);
                        iecReflection.Validate();
                        return iecReflection;
                    });

                    //var uriPrefixes = hostBuilderContext.Configuration.GetSection("PrometheusOptions:Endpoints").Get<string[]>() ?? _uriPrefixes;

                    //services.AddOpenTelemetry().WithMetrics(pb =>
                    //{
                    //    pb
                    //    .ConfigureResource(r => r.AddService(serviceName: serviceName))
                    //    .AddRuntimeInstrumentation()
                    //    .AddProcessInstrumentation()
                    //    .AddMeter(IEC104Diagnostic.MeterName)
                    //    .AddEventCountersInstrumentation(c =>
                    //        c.AddEventSources("System.Net.Sockets"))
                    //    .AddPrometheusHttpListener(
                    //        options =>
                    //        {
                    //            options.UriPrefixes = uriPrefixes;
                    //            options.ScrapeEndpointPath = "/metrics";
                    //            options.DisableTotalNameSuffixForCounters = true;
                    //        });
                    //});

                    services.AddSingleton<IIEC60870_5_104ChannelLayerDiagnostic, IEC60870_5_104ChannelLayerEmptyDiagnostic>();

                    services.AddHostedService<IEC104ClientsStarterService>();
                })
                .ConfigureLogging((hostBuilderContext, logging) =>
                {
                    logging.ClearProviders();
                    //var serviceName = hostBuilderContext.Configuration.GetValue("ServiceName", "IEC104Export");
                    LogManager.Configuration = new NLogLoggingConfiguration(hostBuilderContext.Configuration.GetSection("NLog"));
                    //var fileTargetConfig = LogManager.Configuration.FindTargetByName<AsyncTargetWrapper>("logfile");
                    //var enviromentManager = EnviromentManagerDiExtension.GetEnviromentManager(serviceName);
                    //if (fileTargetConfig!.WrappedTarget is FileTarget fileTarget)
                    //{
                    //    fileTarget.FileName = Path.Combine(enviromentManager.GetLogPath(), "current.log");
                    //    fileTarget.ArchiveFileName = Path.Combine(enviromentManager.GetLogPath(), "{#}.log");
                    //}
                })
                .UseNLog();

                var host = builder.Build();

                //using (var scope = host.Services.CreateScope())
                //{
                //    using var db = scope.ServiceProvider.GetRequiredService<IPowerUnitIEC104ServerDbContext>();
                //    await db.Database.MigrateAsync();
                //}

                await host.RunAsync();
                break;
            }
            catch (OperationCanceledException)
            {
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
