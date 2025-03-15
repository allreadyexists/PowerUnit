namespace PowerUnit.Service.IEC104;

public record BaseValue(string SourceId, string EquipmentId, string ParameterId, DateTime? ValueDt, DateTime RegistrationDt);

