using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Web;

namespace PowerUnit;

internal sealed class Program
{
    private const string SERVICE_NAME = "PowerUnitIEC104ExportService";

    private static async Task Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

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

            services.AddSingleton(TimeProvider.System);

            services.AddEnviromentManager(SERVICE_NAME);

            services.AddPowerUnitDbContext(hostBuilderContext.Configuration);

            // внешний
            services.AddScoped<IDataProvider, DataProvider>();
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

        await host.RunAsync();
    }
}
