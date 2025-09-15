using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104;

public class MapValueItem
{
    public ushort Address { get; }
    public ASDUType Type { get; }
    public BaseValue Value { get; set; }
    public MapValueItem(ushort address, ASDUType type)
    {
        Address = address;
        Type = type;
    }
}

