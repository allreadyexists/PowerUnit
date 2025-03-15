namespace PowerUnit.Service.IEC104.Types.Asdu;

[Flags]
public enum QCC : byte
{
    GROUP1 = 1,
    GROUP2 = 2,
    GROUP3 = 3,
    GROUP4 = 4,
    COMMON = 5,
    FIX = 1 << 6,
    FIX_AND_RESET = 1 << 7,
    RESET = 3 << 6
}

