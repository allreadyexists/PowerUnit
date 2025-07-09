using Microsoft.EntityFrameworkCore;

using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

using PowerUnit.Infrastructure.IEC104ServerDb;

internal sealed class Program
{
    private static async Task ApplyMigrations(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PowerUnitIEC104ServerDbContext>();
        await db.Database.MigrateAsync();
    }

    private static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.UseDefaultServiceProvider((context, options) => { options.ValidateScopes = true; })
            .ConfigureServices((hostBuilderContext, services) =>
            {
                services.Configure<HostOptions>(hostOptions =>
                {
                    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
                });

                services.AddPowerUnitIEC104ServerDbContext(hostBuilderContext.Configuration);
            })
            .ConfigureLogging((hostBuilderContext, logging) =>
            {
                logging.ClearProviders();
                LogManager.Configuration = new NLogLoggingConfiguration(hostBuilderContext.Configuration.GetSection("NLog"));
            })
            .UseNLog();

        var host = builder.Build();

        await ApplyMigrations(host);

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (obj, args) =>
        {
            logger.LogInformation("Cancel application");
            cts.Cancel();
        };

        logger.LogInformation("Start application");

        using var scope = host.Services.CreateAsyncScope();
        {
            using var dbContext = scope.ServiceProvider.GetRequiredService<PowerUnitIEC104ServerDbContext>();
            //var result1 = await dbContext.MeasurementTypes.AsNoTracking().Select(x => x).ToArrayAsync(cts.Token);
            //if (result1.Length > 0)
            //{
            //}

            //var query = dbContext.ParameterTypes.AsNoTracking()
            //    .Include(x => x.DiscretType)
            //    .Include(x => x.MeasurementType).OrderBy(x => x.Id).Select(x => new { x.Id, x.Description, DiscretDescription = x.DiscretType.Description, MeasurementDescription = x.MeasurementType.Description });

            //var result2 = await query.ToArrayAsync(cts.Token);
            //foreach (var r2 in result2)
            //{
            //    logger.LogInformation($"{r2.Id} {r2.Description} {r2.DiscretDescription} {r2.MeasurementDescription}");
            //}

            //var serverId = 1;
            //var address = 105;
            //var result3 = await dbContext.IEC104Mappings.AsNoTracking().Include(x => x.Equipment).Where(x => x.ServerId == serverId && x.Address == address)
            //    .Join(dbContext.Measurements.AsNoTracking(), u => u.EquipmentId, v => v.EquipmentId, (u, v) => new { u.IEC104TypeId, u.Address, v.Value, v.ValueDt, v.RegistrationDt }).OrderByDescending(x => x.ValueDt).FirstOrDefaultAsync();

            //if (result3 != null)
            //{
            //    logger.LogInformation($"{result3.IEC104TypeId} {result3.Address} {result3.Value} {result3.ValueDt} {result3.RegistrationDt}");
            //}

            var servers = await dbContext.IEC104Servers.AsNoTracking().Include(x => x.ChannelLayerOption).ToArrayAsync();
            foreach (var server in servers)
            {

            }
        }

        Console.ReadKey();
        logger.LogInformation("Stop application");
    }
}
