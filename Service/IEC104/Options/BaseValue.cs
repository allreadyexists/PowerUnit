namespace PowerUnit.Service.IEC104;

public record BaseValue(long EquipmentId, long ParameterId, DateTime? ValueDt, DateTime RegistrationDt);

