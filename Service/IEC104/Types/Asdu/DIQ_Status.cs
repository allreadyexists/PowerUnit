namespace PowerUnit.Service.IEC104.Types.Asdu;

[Flags]
public enum DIQ_Status : byte
{
    BL = 1 << 4,
    SB = 1 << 5,
    NT = 1 << 6,
    IV = 1 << 7,
}

