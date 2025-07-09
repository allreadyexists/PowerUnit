//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace PowerUnit;

//public enum MeasurementTypeEnum : byte
//{
//    Instant,
//    Current,
//    Integral,
//    Differential
//}

///// <summary>
///// Тип измеряемого параметра: мгновенное значение, интегральное, дифференциальное
///// </summary>
//public class MeasurementTypeItem : IEntityTypeConfiguration<MeasurementTypeItem>, IEntityWithDescription
//{
//    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
//    public MeasurementTypeEnum Id { get; set; }
//    public string Description { get; set; }

//    void IEntityTypeConfiguration<MeasurementTypeItem>.Configure(EntityTypeBuilder<MeasurementTypeItem> builder)
//    {
//        builder.ConfigureDiscription();
//    }
//}
