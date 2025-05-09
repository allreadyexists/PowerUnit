using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit;

public class IEC104ServerItem : IEntityTypeConfiguration<IEC104ServerItem>, IEntityWithName, IEntityEnabled
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Port { get; set; }
    public ushort CommonASDUAddress { get; set; }
    public bool CheckCommonASDUAddress { get; set; }
    public bool SporadicSendEnabled { get; set; }
    public int FileSectionSize { get; set; }
    public byte FileSegmentSize { get; set; }
    public bool Enable { get; set; }
    public IEC104ServerChannelLayerOptionItem ChannelLayerOption { get; set; }

    void IEntityTypeConfiguration<IEC104ServerItem>.Configure(EntityTypeBuilder<IEC104ServerItem> builder)
    {
        builder.ConfigureName();
        builder.ConfigureEnabled();
        builder.Property(p => p.Port).IsRequired().HasDefaultValue(2404);
        builder.Property(p => p.CommonASDUAddress).IsRequired().HasDefaultValue(1);
        builder.Property(p => p.SporadicSendEnabled).HasDefaultValue(false);
        builder.Property(p => p.FileSectionSize).HasDefaultValue(1024).HasMaxLength(4096);
        builder.Property(p => p.FileSegmentSize).HasDefaultValue(200).HasMaxLength(200);
        builder.Property(p => p.CheckCommonASDUAddress).HasDefaultValue(true);

        builder.HasOne(p => p.ChannelLayerOption).WithOne().HasForeignKey<IEC104ServerChannelLayerOptionItem>(s => s.Id).OnDelete(DeleteBehavior.Cascade);
    }
}
