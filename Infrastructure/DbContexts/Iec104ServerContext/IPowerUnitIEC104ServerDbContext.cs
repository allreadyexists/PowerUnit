using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public interface IPowerUnitIEC104ServerDbContext : IDisposable
{
    DatabaseFacade Database { get; }

    /// <summary>
    /// НСИ МЭК104 группы
    /// </summary>
    DbSet<IEC104GroupItem> Groups { get; set; }
    /// <summary>
    /// НСИ МЭК104 маппинги
    /// </summary>
    DbSet<IEC104MappingItem> Mappings { get; set; }
    /// <summary>
    /// НСИ МЭК104 сервера
    /// </summary>
    DbSet<IEC104ServerItem> Servers { get; set; }
    /// <summary>
    /// Настройки МЭК104 сервера
    /// </summary>
    DbSet<IEC104ServerApplicationLayerOptionItem> ApplicationLayerOptions { get; set; }
    /// <summary>
    /// Настройки МЭК104 сервера
    /// </summary>
    DbSet<IEC104ServerChannelLayerOptionItem> ChannelLayerOptions { get; set; }
    /// <summary>
    /// НСИ МЭК104 типы
    /// </summary>
    DbSet<IEC104TypeItem> Types { get; set; }
}
