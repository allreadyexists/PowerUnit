using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog;
using NLog.Web;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using PowerUnit.Common.EnviromentManager;
using PowerUnit.Common.Subsciption;
using PowerUnit.Common.TimeoutService;
using PowerUnit.DataSource.Test;
using PowerUnit.Infrastructure.IEC104ServerDb.PostgreSql;
using PowerUnit.Infrastructure.IEC104ServerDb.Sqlite;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;

using System.Reflection;

using ILogger = NLog.ILogger;

namespace PowerUnit.Service.IEC104.Export;

internal sealed class Program
{
    private static readonly string[] _uriPrefixes = ["http://localhost:5000/"];

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IHost? CreateHost(string[] args, ILogger? logger)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.Configure<HostOptions>(hostOptions => hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost);

        var serviceName = builder.Configuration.GetValue("ServiceName", "IEC104Export");

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddEnviromentManager(serviceName);

        var dbProvider = builder.Configuration.GetValue("DbProvider", "Sqlite");
        switch (dbProvider)
        {
            case "Sqlite":
                builder.Services.AddPowerUnitIEC104ServerDbContextSqlite(builder.Configuration);
                break;
            case "PostgreSql":
                builder.Services.AddPowerUnitIEC104ServerPostgreSqlDbContext(builder.Configuration);
                break;
            default:
                logger?.Error($"Unsupported db provider: {dbProvider}");
                return null;
        }

        builder.Services.AddBaseValueTestDataSource(builder.Configuration);

        builder.Services.AddSingleton<IConfigProvider, IEC104ConfigProvider>();

        builder.Services.AddSingleton<IEC104ServerFactory>();
        builder.Services.AddTimeoutService(ServiceLifetime.Transient);
        builder.Services.AddSingleton<IDataProvider, IEC104DataProvider>();

        builder.Services.AddSingleton<IEC104Diagnostic>();
        builder.Services.AddSingleton<IIEC60870_5_104ChannelLayerDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());
        builder.Services.AddSingleton<IIEC60870_5_104ApplicationLayerDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());
        builder.Services.AddSingleton<ITimeoutServiceDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());
        builder.Services.AddSingleton<ITestDataSourceDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());
        builder.Services.AddSingleton<ISubscriberDiagnostic>(sp => sp.GetRequiredService<IEC104Diagnostic>());

        builder.Services.AddSingleton(s =>
        {
            var iecReflection = ActivatorUtilities.CreateInstance<IECParserGenerator>(s, [Array.Empty<Assembly>()]);
            iecReflection.Validate();
            return iecReflection;
        });

        var uriPrefixes = builder.Configuration.GetSection("PrometheusOptions:Endpoints").Get<string[]>() ?? _uriPrefixes;

        builder.Services.AddOpenTelemetry().WithMetrics(pb =>
        {
            pb
            .ConfigureResource(r => r.AddService(serviceName: serviceName))
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddMeter(IEC104Diagnostic.MeterName)
            .AddEventCountersInstrumentation(c => c.AddEventSources("System.Net.Sockets"))
            .AddPrometheusHttpListener(
                options =>
                {
                    options.UriPrefixes = uriPrefixes;
                    options.ScrapeEndpointPath = "/metrics";
                    options.DisableTotalNameSuffixForCounters = true;
                });
        });

        builder.Services.AddHostedService<IEC104ServersStarterService>();

        builder.AddNLogEx(serviceName);

        return builder.Build();
    }

    private static async Task Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        while (true)
        {
            try
            {
                var host = CreateHost(args, logger);
                if (host == null)
                    break;

                await host.ApplyDbMigrations();

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
