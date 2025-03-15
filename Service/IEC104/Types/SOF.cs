namespace PowerUnit.Service.IEC104.Types;

[Flags]
public enum SOF : byte
{
    LastFileInDirectory = 1 << 5,
    IsSubDirectory = 1 << 6,
    IsActiveTransmit = 1 << 7
}

