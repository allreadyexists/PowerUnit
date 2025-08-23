using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel.Resolution;

using System.Reflection;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

/// <summary>
/// dotnet ef migrations add Migration_/имя миграции/ -c PowerUnitIec104ServerDbContext  -o .\Migrations -p .\PowerUnit.Infrastructure.Db.csproj
/// </summary>
public class PowerUnitIEC104ServerSqliteDbContext : PowerUnitIEC104ServerDbContext
{
    public PowerUnitIEC104ServerSqliteDbContext(DbContextOptions<PowerUnitIEC104ServerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //modelBuilder.HasDefaultSchema(IEC104ServerDbInfo.SCHEMA_NAME);
        //base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
