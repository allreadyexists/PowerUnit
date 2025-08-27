using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public abstract class PowerUnitIEC104ServerDbContext : DbContext, IPowerUnitIEC104ServerDbContext
{
    /// <summary>
    /// НСИ МЭК104 группы
    /// </summary>
    public DbSet<IEC104GroupItem> Groups { get; set; }
    /// <summary>
    /// НСИ МЭК104 маппинги
    /// </summary>
    public DbSet<IEC104MappingItem> Mappings { get; set; }
    /// <summary>
    /// НСИ МЭК104 сервера
    /// </summary>
    public DbSet<IEC104ServerItem> Servers { get; set; }
    /// <summary>
    /// Настройки МЭК104 сервера
    /// </summary>
    public DbSet<IEC104ServerApplicationLayerOptionItem> ApplicationLayerOptions { get; set; }
    /// <summary>
    /// Настройки МЭК104 сервера
    /// </summary>
    public DbSet<IEC104ServerChannelLayerOptionItem> ChannelLayerOptions { get; set; }
    /// <summary>
    /// НСИ МЭК104 типы
    /// </summary>
    public DbSet<IEC104TypeItem> Types { get; set; }

    public PowerUnitIEC104ServerDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
