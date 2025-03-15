using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104ServerApplicationLayerOptionItem : IEntityTypeConfiguration<IEC104ServerApplicationLayerOptionItem>
{
    [Key]
    public int Id { get; set; }
    public bool CheckCommonASDUAddress { get; set; } = true;
    public bool SporadicSendEnabled { get; set; } = true;
    public ICollection<IEC104ServerItem> IEC104ServerItems { get; } = new List<IEC104ServerItem>();

    void IEntityTypeConfiguration<IEC104ServerApplicationLayerOptionItem>.Configure(EntityTypeBuilder<IEC104ServerApplicationLayerOptionItem> builder)
    {
        builder.Property(p => p.SporadicSendEnabled).HasDefaultValue(false);
        builder.Property(p => p.CheckCommonASDUAddress).HasDefaultValue(true);

        builder.HasMany(p => p.IEC104ServerItems).WithOne(p => p.ApplicationLayerOption).HasForeignKey(p => p.ApplicationLayerOptionId).IsRequired(false);
    }
}
