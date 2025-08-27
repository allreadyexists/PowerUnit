using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

/* PowerShell
1. cd .\Infrastructure\DbContexts\Iec104ServerContext.PostgreSqlMigrations\
2. $env:ConnectionString="Host=host.docker.internal;Port=6543;Database=PUDB;Username=postgres;Password=postgres"
3. dotnet-ef migrations add Migration_/XXXXX_имя миграции/-c PowerUnitIEC104ServerPostgreSqlDbContext -o .\Migrations\ --json
*/
public class PowerUnitIEC104ServerDbContextFactory : IDesignTimeDbContextFactory<PowerUnitIEC104ServerPostgreSqlDbContext>
{
    public PowerUnitIEC104ServerPostgreSqlDbContext CreateDbContext(string[] args)
        => new PowerUnitIEC104ServerPostgreSqlDbContext(
            new DbContextOptionsBuilder<PowerUnitIEC104ServerPostgreSqlDbContext>().UseSnakeCaseNamingConvention()
            .UseNpgsql(Environment.GetEnvironmentVariable("connection_string")
            , x =>
            {
                x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb.PostgreSql");
                x.MigrationsHistoryTable("__EFMigrationsHistory", IEC104ServerDbInfo.SCHEMA_NAME);
            }).Options);

}
