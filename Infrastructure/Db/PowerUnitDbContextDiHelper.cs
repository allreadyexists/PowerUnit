//using Microsoft.EntityFrameworkCore;

//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Options;

//using Npgsql;

//using System.Data.Common;

//namespace PowerUnit;

//public static class PowerUnitDbContextDiHelper
//{
//    public static IServiceCollection AddPowerUnitDbContext(this IServiceCollection services, IConfiguration config)
//    {
//        services.AddOptions<DbOptions>().Bind(config.GetSection(nameof(DbOptions)));
//        services.AddSingleton<DbConnectionStringBuilder>(p =>
//        {
//            var dbOptions = p.GetRequiredService<IOptions<DbOptions>>().Value;
//            var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
//            {
//                Host = dbOptions.Host,
//                Port = dbOptions.Port,
//                Database = dbOptions.Database,
//                Username = dbOptions.User,
//                Password = dbOptions.Password
//            };
//            return connectionStringBuilder;
//        });
//        services.AddDbContextPool<PowerUnitDbContext>((p, x) => x
//            .UseNpgsql(p.GetRequiredService<DbConnectionStringBuilder>().ConnectionString)
//            .UseSnakeCaseNamingConvention()
//            .EnableSensitiveDataLogging()
//        );

//        return services;
//    }
//}
