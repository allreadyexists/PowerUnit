namespace PowerUnit.Asdu;

[Flags]
public enum DIQ_Value : byte
{
    Intermediate = 0,
    On = 1,
    Off = 2,
    Undefined = On | Off
}
