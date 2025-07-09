using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104MappingItem : IEntityTypeConfiguration<IEC104MappingItem>
{
    public long Id { get; set; }

    public int ServerId { get; set; }
    public required IEC104ServerItem Server { get; set; }

    public long EquipmentId { get; set; }
    public long ParameterId { get; set; }

    public int Address { get; set; }
    public IEC104TypeEnum IEC104TypeId { get; set; }
    public required IEC104TypeItem IEC104Type { get; set; }

    void IEntityTypeConfiguration<IEC104MappingItem>.Configure(EntityTypeBuilder<IEC104MappingItem> builder)
    {
        builder.Property(e => e.ServerId).IsRequired();
        builder.Property(e => e.EquipmentId).IsRequired();
        builder.Property(e => e.IEC104TypeId).IsRequired();
        builder.Property(e => e.Address).IsRequired();
        builder.HasOne(e => e.Server).WithMany().HasForeignKey(e => e.ServerId);
        builder.HasOne(e => e.IEC104Type).WithMany().HasForeignKey(e => e.IEC104TypeId);
    }
}
