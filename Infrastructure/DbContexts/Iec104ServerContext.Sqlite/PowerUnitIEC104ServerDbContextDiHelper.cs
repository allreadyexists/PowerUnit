using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PowerUnit.Common.EnviromentManager;
using PowerUnit.Infrastructure.IEC104ServerDb;

using System.Data.Common;

namespace PowerUnit;

public static class PowerUnitIEC104ServerDbContextDiHelper
{
    public static IServiceCollection AddPowerUnitIEC104ServerDbContextSqlite(this IServiceCollection services, IConfiguration config)
    {
        const string SERVICE_NAME = "PowerUnitIEC104ExportService";

        services.AddOptions<IEC104ServerDbSqliteOptions>().Bind(config.GetSection(nameof(IEC104ServerDbSqliteOptions)));
        services.AddSingleton<DbConnectionStringBuilder>(p =>
        {
            var dbOptions = p.GetRequiredService<IOptions<IEC104ServerDbSqliteOptions>>().Value;
            var connectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = Path.Combine(EnviromentManagerDiExtension.GetEnviromentManager(SERVICE_NAME).GetDataPath(), dbOptions.Database)
            };
            return connectionStringBuilder;
        });
        services.AddDbContextPool<IPowerUnitIEC104ServerDbContext, PowerUnitIEC104ServerSqliteDbContext>((p, x) => x
            .UseSqlite(
                p.GetRequiredService<DbConnectionStringBuilder>().ConnectionString,
                x => x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb.Sqlite")
                //x => x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb")
             )
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(false)
            .EnableThreadSafetyChecks()
            .EnableDetailedErrors()
        );

        return services;
    }
}
