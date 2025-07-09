using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Abstract;

public record BaseValue(AsduType AsduType, ushort Address, DateTime? ValueDt, DateTime RegistrationDt, byte/*TODO type*/ Status);
public record AnalogValue(AsduType AsduType, ushort Address, float Value, DateTime? ValueDt, DateTime RegistrationDt, byte/*TODO type*/ Status) : BaseValue(AsduType, Address, ValueDt, RegistrationDt, Status);
public record DiscretValue(AsduType AsduType, ushort Address, bool Value, DateTime? ValueDt, DateTime RegistrationDt, byte/*TODO type*/ Status) : BaseValue(AsduType, Address, ValueDt, RegistrationDt, Status);

public interface IDataProvider
{
    IAsyncEnumerable<AnalogValue> GetAnalogGroup(int serverId, QOI qoi, CancellationToken ct);
    IAsyncEnumerable<DiscretValue> GetDiscretGroup(int serverId, QOI qoi, CancellationToken ct);

    Task<AnalogValue?> GetAnalogValue(int serverId, ushort address, CancellationToken ct);
    Task<DiscretValue?> GetDiscretValue(int serverId, ushort address, CancellationToken ct);
}
