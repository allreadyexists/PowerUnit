using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Npgsql;

using System.Data.Common;

namespace PowerUnit.Infrastructure.IEC104ServerDb.PostgreSql;

public static class PowerUnitIEC104ServerDbContextDiHelper
{
    public static IServiceCollection AddPowerUnitIEC104ServerPostgreSqlDbContext(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<IEC104ServerPostgreSqlDbOptions>().Bind(config.GetSection(nameof(IEC104ServerPostgreSqlDbOptions)));
        services.AddSingleton<DbConnectionStringBuilder>(p =>
        {
            var dbOptions = p.GetRequiredService<IOptions<IEC104ServerPostgreSqlDbOptions>>().Value;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
            {
                Host = dbOptions.Host,
                Port = dbOptions.Port,
                Database = dbOptions.Database,
                Username = dbOptions.User,
                Password = dbOptions.Password
            };
            return connectionStringBuilder;
        });
        services.AddDbContextPool<IPowerUnitIEC104ServerDbContext, PowerUnitIEC104ServerPostgreSqlDbContext>((p, x) => x
            .UseNpgsql(
                p.GetRequiredService<DbConnectionStringBuilder>().ConnectionString,
                x => x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb.PostgreSql")
                //x => x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb")
            )
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(false)
            .EnableThreadSafetyChecks()
            .EnableDetailedErrors()
        );

        return services;
    }

    //public static IServiceCollection AddPowerUnitIEC104ServerDbContextSqlite(this IServiceCollection services, IConfiguration config)
    //{
    //    const string SERVICE_NAME = "PowerUnitIEC104ExportService";

    //    services.AddOptions<IEC104ServerDbSqliteOptions>().Bind(config.GetSection(nameof(IEC104ServerDbSqliteOptions)));
    //    services.AddSingleton<DbConnectionStringBuilder>(p =>
    //    {
    //        var dbOptions = p.GetRequiredService<IOptions<IEC104ServerDbSqliteOptions>>().Value;
    //        var connectionStringBuilder = new SqliteConnectionStringBuilder()
    //        {
    //            DataSource = Path.Combine(EnviromentManagerDiExtension.GetEnviromentManager(SERVICE_NAME).GetDataPath(), dbOptions.Database)
    //        };
    //        return connectionStringBuilder;
    //    });
    //    services.AddDbContextPool<PowerUnitIEC104ServerDbContext>((p, x) => x
    //        .UseSqlite(
    //            p.GetRequiredService<DbConnectionStringBuilder>().ConnectionString,
    //            x => x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb.SqliteMigrations")
    //            //x => x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb")
    //         )
    //        .UseSnakeCaseNamingConvention()
    //        .EnableSensitiveDataLogging(false)
    //        .EnableThreadSafetyChecks()
    //        .EnableDetailedErrors()
    //    );

    //    return services;
    //}
}
