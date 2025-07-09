namespace PowerUnit.Service.IEC104.Types.Asdu;

[Flags]
public enum QDS_Status : byte
{
    OV = 1 << 0,
    BL = 1 << 4,
    SB = 1 << 5,
    NT = 1 << 6,
    IV = 1 << 7,
}

