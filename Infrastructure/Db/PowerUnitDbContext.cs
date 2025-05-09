using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace PowerUnit;

/// <summary>
/// dotnet ef migrations add Migration_/имя миграции/ -c PowerUnitDbContext -o .\Migrations -p .\PowerUnit.Infrastructure.Db.csproj
/// </summary>
public class PowerUnitDbContext : DbContext
{
    /// <summary>
    /// НСИ типы дискретностей
    /// </summary>
    public DbSet<DiscretTypeItem> DiscretTypes { get; set; }
    /// <summary>
    /// НСИ оборудование
    /// </summary>
    public DbSet<EquipmentItem> Equipments { get; set; }
    /// <summary>
    /// НСИ типы оборудования
    /// </summary>
    public DbSet<EquipmentTypeItem> EquipmentTypes { get; set; }
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
    public DbSet<IEC104ServerChannelLayerOptionItem> IEC104ServerChannelLayerOption { get; set; }
    /// <summary>
    /// НСИ МЭК104 типы
    /// </summary>
    public DbSet<IEC104TypeItem> IEC104Types { get; set; }
    /// <summary>
    /// Изменения
    /// </summary>
    public DbSet<MeasurementItem> Measurements { get; set; }
    /// <summary>
    /// НСИ типы измерений
    /// </summary>
    public DbSet<MeasurementTypeItem> MeasurementTypes { get; set; }
    /// <summary>
    /// НСИ типы параметров
    /// </summary>
    public DbSet<ParameterTypeItem> ParameterTypes { get; set; }

    public PowerUnitDbContext(DbContextOptions<PowerUnitDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("power_unit");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
