using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit;

public class EquipmentItem : IEntityTypeConfiguration<EquipmentItem>, IEntityWithDescription, IEntityWithName
{
    public long Id { get; set; }
    public EquipmentTypeEnum EquipmentTypeId { get; set; }
    public EquipmentTypeItem EquipmentType { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Name { get; set; }
    public string Description { get; set; }

    void IEntityTypeConfiguration<EquipmentItem>.Configure(EntityTypeBuilder<EquipmentItem> builder)
    {
        builder.Property(e => e.SerialNumber).HasMaxLength(64).IsRequired();
        builder.ConfigureName();
        builder.ConfigureDiscription();
        builder.HasIndex(e => e.SerialNumber);
        builder.HasIndex(e => new { e.EquipmentTypeId, e.SerialNumber }).IsUnique();
        builder.HasOne(x => x.EquipmentType).WithMany().HasForeignKey(x => x.EquipmentTypeId);
    }
}
