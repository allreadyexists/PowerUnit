using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

//using System.ComponentModel.DataAnnotations;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104MappingItem : IEntityTypeConfiguration<IEC104MappingItem>
{
    //[Key]
    public long Id { get; set; }

    public int ServerId { get; set; }
    public required IEC104ServerItem Server { get; set; }

    public string SourceId { get; set; } = string.Empty;
    public string EquipmentId { get; set; } = string.Empty;
    public string ParameterId { get; set; } = string.Empty;

    public ushort Address { get; set; }
    public IEC104TypeEnum TypeId { get; set; }
    public required IEC104TypeItem Type { get; set; }

    void IEntityTypeConfiguration<IEC104MappingItem>.Configure(EntityTypeBuilder<IEC104MappingItem> builder)
    {
        builder.Property(e => e.ServerId).IsRequired();

        builder.Property(e => e.SourceId).IsRequired().HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(e => e.EquipmentId).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ParameterId).IsRequired().HasMaxLength(512);

        builder.Property(e => e.TypeId).IsRequired();
        builder.Property(e => e.Address).IsRequired();

        builder.HasOne(e => e.Server).WithMany().HasForeignKey(e => e.ServerId);
        builder.HasOne(e => e.Type).WithMany().HasForeignKey(e => e.TypeId);

        builder.HasIndex(e => new { e.ServerId, e.SourceId, e.EquipmentId, e.ParameterId, e.Address, e.TypeId }, "IX_IEC104MappingItem").IsUnique();
    }
}
