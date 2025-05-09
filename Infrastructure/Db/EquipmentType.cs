using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerUnit;

public enum EquipmentTypeEnum : int
{
    Spodes1,
    Spodes3,
}

public class EquipmentTypeItem : IEntityTypeConfiguration<EquipmentTypeItem>, IEntityWithDescription
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public EquipmentTypeEnum Id { get; set; }
    public string Description { get; set; }

    void IEntityTypeConfiguration<EquipmentTypeItem>.Configure(EntityTypeBuilder<EquipmentTypeItem> builder)
    {
        builder.ConfigureDiscription();
    }
}
