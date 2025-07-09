using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

/// <summary>
/// dotnet ef migrations add Migration_/имя миграции/ -c PowerUnitIec104ServerDbContext  -o .\Migrations -p .\PowerUnit.Infrastructure.Db.csproj
/// </summary>
public class PowerUnitIEC104ServerDbContext : DbContext
{
    /// <summary>
    /// НСИ МЭК104 группы
    /// </summary>
    public DbSet<IEC104GroupItem> IEC104Groups { get; set; }
    /// <summary>
    /// НСИ МЭК104 маппинги
    /// </summary>
    public DbSet<IEC104MappingItem> IEC104Mappings { get; set; }
    /// <summary>
    /// НСИ МЭК104 сервера
    /// </summary>
    public DbSet<IEC104ServerItem> IEC104Servers { get; set; }
    /// <summary>
    /// Настройки МЭК104 сервера
    /// </summary>
    public DbSet<IEC104ServerApplicationLayerOptionItem> IEC104ServerApplicationLayerOptions { get; set; }
    /// <summary>
    /// Настройки МЭК104 сервера
    /// </summary>
    public DbSet<IEC104ServerChannelLayerOptionItem> IEC104ServerChannelLayerOptions { get; set; }
    /// <summary>
    /// НСИ МЭК104 типы
    /// </summary>
    public DbSet<IEC104TypeItem> IEC104Types { get; set; }

    public PowerUnitIEC104ServerDbContext(DbContextOptions<PowerUnitIEC104ServerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(IEC104ServerDbInfo.SCHEMA_NAME);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
