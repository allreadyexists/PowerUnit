using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104;

public record MapValueItem(ushort Address, ASDUType Type, BaseValue Value);

