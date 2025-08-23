using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

//using System.ComponentModel.DataAnnotations;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104GroupItem : IEntityTypeConfiguration<IEC104GroupItem>
{
    //[Key]
    public long Id { get; set; }
    public long MappingId { get; set; }
    public required IEC104MappingItem Mapping { get; set; }
    public byte Group { get; set; }

    void IEntityTypeConfiguration<IEC104GroupItem>.Configure(EntityTypeBuilder<IEC104GroupItem> builder)
    {
        builder.Property(e => e.MappingId).IsRequired();
        builder.Property(e => e.Group).IsRequired();
        builder.HasOne(e => e.Mapping).WithMany().HasForeignKey(e => e.MappingId);
        builder.HasIndex(e => e.Group);
    }
}
