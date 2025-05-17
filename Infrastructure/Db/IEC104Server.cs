using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit;

public class IEC104ServerItem : IEntityTypeConfiguration<IEC104ServerItem>, IEntityWithName, IEntityEnabled
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Port { get; set; }
    public ushort CommonASDUAddress { get; set; }
    public bool Enable { get; set; }
    public IEC104ServerApplicationLayerOptionItem? ApplicationLayerOption { get; set; }
    public IEC104ServerChannelLayerOptionItem? ChannelLayerOption { get; set; }

    void IEntityTypeConfiguration<IEC104ServerItem>.Configure(EntityTypeBuilder<IEC104ServerItem> builder)
    {
        builder.ConfigureName();
        builder.ConfigureEnabled();
        builder.Property(p => p.Port).IsRequired().HasDefaultValue(2404);
        builder.Property(p => p.CommonASDUAddress).IsRequired().HasDefaultValue(1);

        builder.HasOne(p => p.ApplicationLayerOption).WithOne().HasForeignKey<IEC104ServerApplicationLayerOptionItem>(s => s.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.ChannelLayerOption).WithOne().HasForeignKey<IEC104ServerChannelLayerOptionItem>(s => s.Id).OnDelete(DeleteBehavior.Cascade);
    }
}
