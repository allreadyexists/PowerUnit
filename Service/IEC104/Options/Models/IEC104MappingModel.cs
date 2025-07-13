namespace PowerUnit.Service.IEC104.Options.Models;

public sealed record class IEC104MappingModel
{
    public int ServerId { get; set; }
    public int Group { get; set; }
    public int Address { get; set; }
    public byte AsduType { get; set; }
    public long EquipmentId { get; set; }
    public long ParameterId { get; set; }
}

