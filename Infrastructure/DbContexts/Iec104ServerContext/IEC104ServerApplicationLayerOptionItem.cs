using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104ServerApplicationLayerOptionItem : IEntityTypeConfiguration<IEC104ServerApplicationLayerOptionItem>
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public bool CheckCommonASDUAddress { get; set; } = true;
    public bool SporadicSendEnabled { get; set; }

    void IEntityTypeConfiguration<IEC104ServerApplicationLayerOptionItem>.Configure(EntityTypeBuilder<IEC104ServerApplicationLayerOptionItem> builder)
    {
        builder.Property(p => p.SporadicSendEnabled).HasDefaultValue(false);
        builder.Property(p => p.CheckCommonASDUAddress).HasDefaultValue(true);
    }
}
