namespace PowerUnit.Service.IEC104.Types.Asdu;

public enum SRQ : byte
{
    Ready = 0,
    NotReady = 1 << 7
}
