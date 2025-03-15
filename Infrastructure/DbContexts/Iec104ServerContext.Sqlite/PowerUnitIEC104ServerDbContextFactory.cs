using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PowerUnit.Infrastructure.IEC104ServerDb.Sqlite;

/* PowerShell
1. cd .\Infrastructure\DbContexts\Iec104ServerContext.SqliteMigrations\
2. $env:ConnectionString="pu_iec104.db"
3. dotnet-ef migrations add Migration_/XXXXX_имя миграции/-c PowerUnitIEC104ServerSqliteDbContext -o .\Migrations\ --json
*/
public class PowerUnitIEC104ServerDbContextFactory : IDesignTimeDbContextFactory<PowerUnitIEC104ServerSqliteDbContext>
{
    public PowerUnitIEC104ServerSqliteDbContext CreateDbContext(string[] args)
        => new PowerUnitIEC104ServerSqliteDbContext(
            new DbContextOptionsBuilder<PowerUnitIEC104ServerSqliteDbContext>().UseSnakeCaseNamingConvention()
            .UseSqlite(Environment.GetEnvironmentVariable("connection_string")
            , x =>
            {
                x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb.Sqlite");
                x.MigrationsHistoryTable("__EFMigrationsHistory");
            }).Options);

}
