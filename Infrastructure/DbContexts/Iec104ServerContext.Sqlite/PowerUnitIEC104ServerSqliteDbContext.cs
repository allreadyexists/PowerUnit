using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PowerUnit.Infrastructure.IEC104ServerDb.Sqlite;

public sealed class PowerUnitIEC104ServerSqliteDbContext : PowerUnitIEC104ServerDbContext
{
    public PowerUnitIEC104ServerSqliteDbContext(DbContextOptions<PowerUnitIEC104ServerSqliteDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(bool))
                {
                    property.SetColumnType("INTEGER");
                    property.SetValueConverter(new BoolToZeroOneConverter<int>());
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
