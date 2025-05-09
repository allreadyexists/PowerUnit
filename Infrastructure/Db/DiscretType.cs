using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PowerUnit;

public enum DiscretTypeEnum : byte
{
    None,
    DayFix,
    MonthFix,
    YearFix,
    Min1,
    Min2,
    Min3,
    Min4,
    Min5,
    Min6,
    Min10,
    Min12,
    Min15,
    Min20,
    Min30,
    Hour1
}

public class DiscretTypeItem : IEntityTypeConfiguration<DiscretTypeItem>, IEntityWithDescription
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public DiscretTypeEnum Id { get; set; }
    public string Description { get; set; }

    void IEntityTypeConfiguration<DiscretTypeItem>.Configure(EntityTypeBuilder<DiscretTypeItem> builder)
    {
        builder.ConfigureDiscription();
    }
}
