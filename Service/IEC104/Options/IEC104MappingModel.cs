namespace PowerUnit.Service.IEC104;

public sealed record class IEC104MappingModel
{
    public int ServerId { get; set; }
    public byte Group { get; set; }
    public ushort Address { get; set; }
    public byte AsduType { get; set; }
    public string SourceId { get; set; }
    public string EquipmentId { get; set; }
    public string ParameterId { get; set; }
}

