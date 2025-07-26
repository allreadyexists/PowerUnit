using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

/* PowerShell
1. cd ./PowerUnit.Infrastructure.Iec104ServerDb
2. $env:ConnectionString="Host=host.docker.internal;Port=6543;Database=PUDB;Username=postgres;Password=postgres"
3. dotnet-ef migrations add Migration_/имя миграции/ -c PowerUnitIEC104ServerDbContext -o .\Migrations
*/

public class PowerUnitIEC104ServerDbContextFactory : IDesignTimeDbContextFactory<PowerUnitIEC104ServerDbContext>
{
    public PowerUnitIEC104ServerDbContext CreateDbContext(string[] args)
        => new PowerUnitIEC104ServerDbContext(new DbContextOptionsBuilder<PowerUnitIEC104ServerDbContext>().UseSnakeCaseNamingConvention().UseNpgsql(Environment.GetEnvironmentVariable("connection_string"),
            x =>
            {
                x.MigrationsAssembly("PowerUnit.Infrastructure.IEC104ServerDb");
                x.MigrationsHistoryTable("__EFMigrationsHistory", IEC104ServerDbInfo.SCHEMA_NAME);
            }).Options);

}
