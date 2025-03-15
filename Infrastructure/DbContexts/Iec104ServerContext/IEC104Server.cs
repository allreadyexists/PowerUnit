using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PowerUnit.Infrastructure.Db;

//using System.ComponentModel.DataAnnotations;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104ServerItem : IEntityTypeConfiguration<IEC104ServerItem>, IEntityWithName, IEntityEnabled
{
    //[Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Port { get; set; }
    public ushort CommonASDUAddress { get; set; }
    public bool Enable { get; set; }
    public int? ApplicationLayerOptionId { get; set; }
    public IEC104ServerApplicationLayerOptionItem? ApplicationLayerOption { get; set; }
    public int? ChannelLayerOptionId { get; set; }
    public IEC104ServerChannelLayerOptionItem? ChannelLayerOption { get; set; }

    void IEntityTypeConfiguration<IEC104ServerItem>.Configure(EntityTypeBuilder<IEC104ServerItem> builder)
    {
        builder.ConfigureName();
        builder.ConfigureEnabled();
        builder.Property(p => p.Port).IsRequired().HasDefaultValue(2404);
        builder.Property(p => p.CommonASDUAddress).IsRequired().HasDefaultValue(1);
        builder.Property(p => p.ApplicationLayerOptionId).IsRequired().HasDefaultValue(1);
        builder.Property(p => p.ChannelLayerOptionId).IsRequired().HasDefaultValue(1);

        //builder.HasOne(p => p.ApplicationLayerOption).WithOne(p => p.IEC104ServerItem).HasForeignKey<IEC104ServerApplicationLayerOptionItem>(s => s.Id)/*.OnDelete(DeleteBehavior.Cascade)*/;
        //builder.HasOne(p => p.ChannelLayerOption).WithOne(p => p.IEC104ServerItem).HasForeignKey<IEC104ServerChannelLayerOptionItem>(s => s.Id)/*.OnDelete(DeleteBehavior.Cascade)*/;
    }
}
