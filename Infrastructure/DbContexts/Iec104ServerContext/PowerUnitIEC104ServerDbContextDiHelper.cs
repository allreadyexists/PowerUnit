using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Npgsql;

using System.Data.Common;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public static class PowerUnitIEC104ServerDbContextDiHelper
{
    public static IServiceCollection AddPowerUnitIEC104ServerDbContext(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<IEC104ServerDbOptions>().Bind(config.GetSection(nameof(IEC104ServerDbOptions)));
        services.AddSingleton<DbConnectionStringBuilder>(p =>
        {
            var dbOptions = p.GetRequiredService<IOptions<IEC104ServerDbOptions>>().Value;
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
        services.AddDbContextPool<PowerUnitIEC104ServerDbContext>((p, x) => x
            .UseNpgsql(p.GetRequiredService<DbConnectionStringBuilder>().ConnectionString)
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(false)
            .EnableThreadSafetyChecks()
            .EnableDetailedErrors()
        );

        return services;
    }

    public static IServiceCollection AddPowerUnitIEC104ServerDbContextSqlite(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<IEC104ServerDbSqliteOptions>().Bind(config.GetSection(nameof(IEC104ServerDbSqliteOptions)));
        services.AddSingleton<DbConnectionStringBuilder>(p =>
        {
            var dbOptions = p.GetRequiredService<IOptions<IEC104ServerDbSqliteOptions>>().Value;
            var connectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = dbOptions.DataSource
            };
            return connectionStringBuilder;
        });
        services.AddDbContextPool<PowerUnitIEC104ServerDbContext>((p, x) => x
            .UseSqlite(
                p.GetRequiredService<DbConnectionStringBuilder>().ConnectionString,
                x => x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb.SqliteMigrations"))
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(false)
            .EnableThreadSafetyChecks()
            .EnableDetailedErrors()
        );

        return services;
    }
}
