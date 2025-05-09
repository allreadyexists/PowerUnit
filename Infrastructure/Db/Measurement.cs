using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit;

public class MeasurementItem : IEntityTypeConfiguration<MeasurementItem>
{
    public long Id { get; set; }

    public long EquipmentId { get; set; }
    public EquipmentItem Equipment { get; set; }

    public ParameterTypeEnum ParameterTypeId { get; set; }
    public ParameterTypeItem ParameterType { get; set; }

    public DateTime ValueDt { get; set; }
    public DateTime RegistrationDt { get; set; } = DateTime.UtcNow;
    public double Value { get; set; }

    void IEntityTypeConfiguration<MeasurementItem>.Configure(EntityTypeBuilder<MeasurementItem> builder)
    {
        builder.Property(e => e.EquipmentId).IsRequired();
        builder.Property(e => e.ParameterTypeId).IsRequired();

        builder.Property(e => e.ValueDt).IsRequired();
        builder.Property(e => e.RegistrationDt).IsRequired();

        builder.HasOne(e => e.ParameterType).WithMany().HasForeignKey(e => e.ParameterTypeId);
        builder.HasOne(e => e.Equipment).WithMany().HasForeignKey(e => e.EquipmentId);

        builder.HasIndex(e => new { e.ValueDt, e.EquipmentId, e.ParameterTypeId }).IsUnique();
    }
}
