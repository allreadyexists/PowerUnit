namespace PowerUnit.Service.IEC104;

public record DiscretValue(long EquipmentId, long ParameterId, bool Value, DateTime? ValueDt, DateTime RegistrationDt) : BaseValue(EquipmentId, ParameterId, ValueDt, RegistrationDt);

