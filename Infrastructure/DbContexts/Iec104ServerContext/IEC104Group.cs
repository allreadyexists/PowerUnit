using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104GroupItem : IEntityTypeConfiguration<IEC104GroupItem>
{
    public long Id { get; set; }
    public long IEC104MappingId { get; set; }
    public required IEC104MappingItem IEC104Mapping { get; set; }
    public byte Group { get; set; }

    void IEntityTypeConfiguration<IEC104GroupItem>.Configure(EntityTypeBuilder<IEC104GroupItem> builder)
    {
        builder.Property(e => e.IEC104MappingId).IsRequired();
        builder.Property(e => e.Group).IsRequired();
        builder.HasOne(e => e.IEC104Mapping).WithMany().HasForeignKey(e => e.IEC104MappingId);
        builder.HasIndex(e => e.Group);
    }
}
