using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Wrappers;

using PowerUnit.Common.EnviromentManager;
using PowerUnit.Infrastructure.IEC104ServerDb;

namespace PowerUnit.Service.IEC104.Export;

internal static class HostHelper
{
    public static async Task ApplyDbMigrations(this IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        using var db = scope.ServiceProvider.GetRequiredService<IPowerUnitIEC104ServerDbContext>();
        await db.Database.MigrateAsync();
    }

    public static void AddNLogEx(this HostApplicationBuilder builder, string serviceName)
    {
        builder.Logging.ClearProviders();

        LogManager.Configuration = new NLogLoggingConfiguration(builder.Configuration.GetSection("NLog"));
        var enviromentManager = EnviromentManagerDiExtension.GetEnviromentManager(serviceName);

        foreach (var target in LogManager.Configuration.AllTargets)
        {
            if (target is AsyncTargetWrapper asyncTargetWrapper && asyncTargetWrapper.WrappedTarget is FileTarget fileTarget)
            {
                if (fileTarget.FileName != null)
                {
                    var logFileName = fileTarget.FileName.ToString()?.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) ?? string.Empty;
                    if (!string.IsNullOrEmpty(logFileName))
                        fileTarget.FileName = Path.Combine(enviromentManager.GetLogPath(), logFileName);
                }
            }
        }

        builder.Logging.AddNLog();
    }
}
