using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104ServerChannelLayerOptionItem : IEntityTypeConfiguration<IEC104ServerChannelLayerOptionItem>
{
    [Key]
    public int Id { get; set; }
    /// <summary>
    /// Таймаут при установки соединения
    /// </summary>
    public byte Timeout0Sec { get; set; } = 30;
    /// <summary>
    /// Таймаут при посылке или тестировании APDU
    /// </summary>
    public byte Timeout1Sec { get; set; } = 15;
    /// <summary>
    /// Таймаут для подтверждения в случае отсутствия сообщения с данными Timeout2Sec < Timeout1Sec
    /// </summary>
    public byte Timeout2Sec { get; set; } = 10;
    /// <summary>
    /// Таймаут для посылки блоков тестирования в случае долгого простоя
    /// </summary>
    public byte Timeout3Sec { get; set; } = 20;

    /// <summary>
    /// Максимальная разность между переменной состояния передачи и номером последнего подтвержденного APDU
    /// </summary>
    public ushort WindowKSize { get; set; } = 12;
    /// <summary>
    ///  Последнее подтверждение после приема W APDU формата I. Рекомендуется <= 2/3 * WindowKSize
    /// </summary>
    public ushort WindowWSize { get; set; } = 8;

    /// <summary>
    /// Использовать фрагментированную отправка ответов
    /// </summary>
    public bool UseFragmentSend { get; set; }

    /// <summary>
    /// Максимальное количество элементов в очереди, после которого низкоприоритетные пакеты
    /// от прикладного уровня будут выкинуты каналкой
    /// </summary>
    public int MaxQueueSize { get; set; }
    public ICollection<IEC104ServerItem> ServerItems { get; } = new List<IEC104ServerItem>();

    void IEntityTypeConfiguration<IEC104ServerChannelLayerOptionItem>.Configure(EntityTypeBuilder<IEC104ServerChannelLayerOptionItem> builder)
    {
        builder.Property(p => p.Timeout0Sec).HasDefaultValue(30);
        builder.Property(p => p.Timeout1Sec).HasDefaultValue(15);
        builder.Property(p => p.Timeout2Sec).HasDefaultValue(10);
        builder.Property(p => p.Timeout3Sec).HasDefaultValue(20);

        builder.Property(p => p.WindowKSize).HasDefaultValue(12);
        builder.Property(p => p.WindowWSize).HasDefaultValue(8);

        builder.Property(p => p.UseFragmentSend).HasDefaultValue(false);

        builder.Property(p => p.MaxQueueSize).HasDefaultValue(100);

        builder.HasMany(p => p.ServerItems).WithOne(p => p.ChannelLayerOption).HasForeignKey(p => p.ChannelLayerOptionId).IsRequired(false);
    }
}
