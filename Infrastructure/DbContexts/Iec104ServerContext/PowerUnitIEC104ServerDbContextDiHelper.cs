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
            .EnableSensitiveDataLogging()
        );

        return services;
    }
}
