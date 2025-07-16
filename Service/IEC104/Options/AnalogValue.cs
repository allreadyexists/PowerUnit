namespace PowerUnit.Service.IEC104;

public record AnalogValue(long EquipmentId, long ParameterId, float Value, DateTime? ValueDt, DateTime RegistrationDt) : BaseValue(EquipmentId, ParameterId, ValueDt, RegistrationDt);

