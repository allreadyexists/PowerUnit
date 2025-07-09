namespace PowerUnit.Service.IEC104.Types.Asdu;

public enum FRQ : byte
{
    Positive = 0,
    Negative = 1 << 7
}
