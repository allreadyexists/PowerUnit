//using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Abstract;

public record BaseValue(long EquipmentId, long ParameterId, /*AsduType AsduType, ushort Address,*/ DateTime? ValueDt, DateTime RegistrationDt);
public record AnalogValue(long EquipmentId, long ParameterId, /*AsduType AsduType, ushort Address,*/ float Value, DateTime? ValueDt, DateTime RegistrationDt) : BaseValue(EquipmentId, ParameterId, /*AsduType, Address,*/ ValueDt, RegistrationDt);
public record DiscretValue(long EquipmentId, long ParameterId, bool Value, DateTime? ValueDt, DateTime RegistrationDt) : BaseValue(EquipmentId, ParameterId, ValueDt, RegistrationDt);

//public interface IDataProvider
//{
//    IAsyncEnumerable<AnalogValue> GetAnalogGroup(int serverId, QOI qoi, CancellationToken ct);
//    IAsyncEnumerable<DiscretValue> GetDiscretGroup(int serverId, QOI qoi, CancellationToken ct);

//    Task<AnalogValue?> GetAnalogValue(int serverId, ushort address, CancellationToken ct);
//    Task<DiscretValue?> GetDiscretValue(int serverId, ushort address, CancellationToken ct);
//}
