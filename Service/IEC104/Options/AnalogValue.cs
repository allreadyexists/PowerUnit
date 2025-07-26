namespace PowerUnit.Service.IEC104;

public record AnalogValue(string SourceId, string EquipmentId, string ParameterId, float Value, DateTime? ValueDt, DateTime RegistrationDt) : BaseValue(SourceId, EquipmentId, ParameterId, ValueDt, RegistrationDt);

