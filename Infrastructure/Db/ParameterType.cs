using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PowerUnit;

public enum ParameterTypeEnum : int
{
    Freq,

    Voltage,
    VoltagePhaseA,
    VoltagePhaseB,
    VoltagePhaseC,

    Current,
    CurrentPhaseA,
    CurrentPhaseB,
    CurrentPhaseC,
}

/// <summary>
/// Типы параметров
/// </summary>
public class ParameterTypeItem : IEntityTypeConfiguration<ParameterTypeItem>, IEntityWithDescription
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ParameterTypeEnum Id { get; set; }
    public string Description { get; set; }

    public MeasurementTypeEnum MeasurementTypeId { get; set; }
    public MeasurementTypeItem MeasurementType { get; set; }
    public DiscretTypeEnum DiscretTypeId { get; set; }
    public DiscretTypeItem DiscretType { get; set; }

    void IEntityTypeConfiguration<ParameterTypeItem>.Configure(EntityTypeBuilder<ParameterTypeItem> builder)
    {
        builder.ConfigureDiscription();
        builder.HasOne(x => x.MeasurementType).WithMany().HasForeignKey(x => x.MeasurementTypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.DiscretType).WithMany().HasForeignKey(x => x.DiscretTypeId).OnDelete(DeleteBehavior.Cascade);
    }
}
