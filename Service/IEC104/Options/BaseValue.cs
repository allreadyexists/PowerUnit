namespace PowerUnit.Service.IEC104;

public record struct BaseValue(string SourceId, string EquipmentId, string ParameterId, DateTime? ValueDt, DateTime RegistrationDt,
    float? ValueAsFloat, bool? ValueAsBool);

