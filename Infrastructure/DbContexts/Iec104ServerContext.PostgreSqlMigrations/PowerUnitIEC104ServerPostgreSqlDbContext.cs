using Microsoft.EntityFrameworkCore;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public sealed class PowerUnitIEC104ServerPostgreSqlDbContext : PowerUnitIEC104ServerDbContext
{
    public PowerUnitIEC104ServerPostgreSqlDbContext(DbContextOptions<PowerUnitIEC104ServerPostgreSqlDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(IEC104ServerDbInfo.SCHEMA_NAME);
    }
}
