namespace PowerUnit.Asdu;

public enum SRQ : byte
{
    Ready = 0,
    NotReady = 1 << 7
}
