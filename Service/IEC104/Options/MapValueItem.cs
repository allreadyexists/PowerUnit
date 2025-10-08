using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104;

public struct MapValueItem
{
    public readonly ushort Address;
    public readonly ASDUType Type;
    public BaseValue Value;
    public MapValueItem(ushort address, ASDUType type)
    {
        Address = address;
        Type = type;
    }
}

