//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;

//namespace PowerUnit;

//public class IEC104ServerApplicationLayerOptionItem : IEntityTypeConfiguration<IEC104ServerApplicationLayerOptionItem>
//{
//    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
//    public int Id { get; set; }
//    public bool CheckCommonASDUAddress { get; set; } = true;
//    public bool SporadicSendEnabled { get; set; }
//    // public int FileSectionSize { get; set; }
//    public byte FileSegmentSize { get; set; } = 200;

//    void IEntityTypeConfiguration<IEC104ServerApplicationLayerOptionItem>.Configure(EntityTypeBuilder<IEC104ServerApplicationLayerOptionItem> builder)
//    {
//        builder.Property(p => p.SporadicSendEnabled).HasDefaultValue(false);
//        // builder.Property(p => p.FileSectionSize).HasDefaultValue(1024).HasMaxLength(4096);
//        builder.Property(p => p.FileSegmentSize).HasDefaultValue(200).HasMaxLength(200);
//        builder.Property(p => p.CheckCommonASDUAddress).HasDefaultValue(true);
//    }
//}
