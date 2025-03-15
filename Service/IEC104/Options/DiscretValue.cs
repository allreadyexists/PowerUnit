namespace PowerUnit.Service.IEC104;

public record DiscretValue(string SourceId, string EquipmentId, string ParameterId, bool Value, DateTime? ValueDt, DateTime RegistrationDt) : BaseValue(SourceId, EquipmentId, ParameterId, ValueDt, RegistrationDt);

