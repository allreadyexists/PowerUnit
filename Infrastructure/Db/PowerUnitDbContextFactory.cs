using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PowerUnit;

/* PowerShell
1. cd ./PowerUnit.Infrastructure.Db
2. $env:ConnectionString="Host=host.docker.internal;Port=6543;Database=PUDB;Username=postgres;Password=postgres"
3. dotnet-ef migrations add Migration_/имя миграции/ -c PowerUnitDbContext -o .\Migrations
*/

public class PowerUnitDbContextFactory : IDesignTimeDbContextFactory<PowerUnitDbContext>
{
    public PowerUnitDbContext CreateDbContext(string[] args)
        => new PowerUnitDbContext(new DbContextOptionsBuilder<PowerUnitDbContext>().UseSnakeCaseNamingConvention().UseNpgsql(Environment.GetEnvironmentVariable("connection_string"),
            x =>
            {
                x.MigrationsAssembly("PowerUnit.Infrastructure.Db");
                x.MigrationsHistoryTable("__EFMigrationsHistory", "public");
            }).Options);

}
