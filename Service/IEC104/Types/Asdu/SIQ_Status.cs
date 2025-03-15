namespace PowerUnit.Service.IEC104.Types.Asdu;

/// <summary>
/// Одноэлементная информация статус, определенная в 7.2.6.1
/// </summary>
[Flags]
public enum SIQ_Status : byte
{
    BL = 0b00010000,
    SB = 0b00100000,
    NT = 0b01000000,
    IV = 0b10000000
}