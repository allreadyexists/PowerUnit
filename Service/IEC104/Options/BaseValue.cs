namespace PowerUnit.Service.IEC104;

public class BaseValue
{
    public string SourceId { get; set; }
    public string EquipmentId { get; set; }
    public string ParameterId { get; set; }
    public DateTime? ValueDt { get; set; }
    public DateTime RegistrationDt { get; set; }
    public float? ValueAsFloat { get; set; }
    public bool? ValueAsBool { get; set; }
}


