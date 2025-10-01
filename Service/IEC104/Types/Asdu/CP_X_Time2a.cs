using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[Flags]
public enum TimeStatus : int
{
    OK = 0,
    RES1 = 1 << 22,
    IV = 1 << 23,
    SU = 1 << 31
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct CP56Time2aTemplate
{
    [FieldOffset(0)]
    public int T1234;

    [FieldOffset(0)]
    public byte T1;
    [FieldOffset(1)]
    public byte T2;

    [FieldOffset(0)]
    public ushort Ms;

    [FieldOffset(2)]
    public byte T3;

    [FieldOffset(3)]
    public byte T4;
    [FieldOffset(4)]
    public byte T5;
    [FieldOffset(5)]
    public byte T6;
    [FieldOffset(6)]
    public byte T7;

    //public CP56Time2a(DateTime dateTime, TimeStatus timeStatus)
    //{
    //    var dayOfWeek = (byte)dateTime.DayOfWeek;

    //    _ms = (ushort)(dateTime.Millisecond + 1000 * dateTime.Second);
    //    _t3 = (byte)(dateTime.Minute & 0b00111111);
    //    _t4 = (byte)(dateTime.Hour & 0b00011111);
    //    _t5 = (byte)((byte)dateTime.Day | (byte)((dayOfWeek == 0 ? 7 : dayOfWeek) << 5));
    //    _t6 = (byte)(dateTime.Month & 0b00011111);
    //    _t7 = (byte)(dateTime.Year - 2000 & 0b01111111);

    //    if ((timeStatus & TimeStatus.RES1) != 0)
    //        _t1234 |= (int)TimeStatus.RES1;
    //    if ((timeStatus & TimeStatus.IV) != 0)
    //        _t1234 |= (int)TimeStatus.IV;
    //    if ((timeStatus & TimeStatus.SU) != 0)
    //        _t1234 |= (int)TimeStatus.SU;
    //}
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct CP56Time2a
{
    [FieldOffset(0)]
    private readonly int _t1234;

    [FieldOffset(0)]
    private readonly byte _t1;
    [FieldOffset(1)]
    private readonly byte _t2;

    [FieldOffset(0)]
    private readonly ushort _ms;

    [FieldOffset(2)]
    private readonly byte _t3;

    [FieldOffset(3)]
    private readonly byte _t4;
    [FieldOffset(4)]
    private readonly byte _t5;
    [FieldOffset(5)]
    private readonly byte _t6;
    [FieldOffset(6)]
    private readonly byte _t7;

    public CP56Time2a(DateTime dateTime, TimeStatus timeStatus)
    {
        var dayOfWeek = (byte)dateTime.DayOfWeek;

        _ms = (ushort)(dateTime.Millisecond + 1000 * dateTime.Second);
        _t3 = (byte)(dateTime.Minute & 0b00111111);
        _t4 = (byte)(dateTime.Hour & 0b00011111);
        _t5 = (byte)((byte)dateTime.Day | (byte)((dayOfWeek == 0 ? 7 : dayOfWeek) << 5));
        _t6 = (byte)(dateTime.Month & 0b00011111);
        _t7 = (byte)(dateTime.Year - 2000 & 0b01111111);

        if ((timeStatus & TimeStatus.RES1) != 0)
            _t1234 |= (int)TimeStatus.RES1;
        if ((timeStatus & TimeStatus.IV) != 0)
            _t1234 |= (int)TimeStatus.IV;
        if ((timeStatus & TimeStatus.SU) != 0)
            _t1234 |= (int)TimeStatus.SU;
    }

    public DateTime DateTime => new DateTime((_t7 & 0b01111111) + 2000, _t6 & 0b00011111,
        _t5 & 0b00011111, _t4 & 0b00011111, _t3 & 0b00111111, _ms / 1000, _ms % 1000);

    public TimeStatus TimeStatus => (TimeStatus)(_t1234 & 0b1000_0000_1100_0000_0000_0000_0000_0000);
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct CP24Time2a
{
    [FieldOffset(0)]
    private readonly byte _t1;
    [FieldOffset(1)]
    private readonly byte _t2;
    [FieldOffset(2)]
    private readonly byte _t3;

    [FieldOffset(0)]
    private readonly ushort _ms;

    public CP24Time2a(TimeOnly timeOnly, TimeStatus timeStatus)
    {
        //if ((timeStatus & TimeStatus.SU) == TimeStatus.SU)
        //    throw new ArgumentException(nameof(timeStatus));
        _ms = (ushort)(timeOnly.Microsecond + 1000 * timeOnly.Second);
        _t3 = (byte)(timeOnly.Minute & 0b00111111);
        if ((timeStatus & TimeStatus.RES1) != 0)
            _t3 |= 1 << 6;
        if ((timeStatus & TimeStatus.IV) != 0)
            _t3 |= 1 << 7;
    }

    public DateTime DateTime(DateTime now) => new DateTime(now.Year, now.Month, now.Day, now.Hour, _t3 & 0b00111111, _ms / 1000, _ms % 1000);
    public TimeStatus TimeStatus => ((byte)(_t3 & 1 << 6) != 0 ? TimeStatus.RES1 : 0) | ((byte)(_t3 & 1 << 7) != 0 ? TimeStatus.IV : 0);
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct CP16Time2a
{
    [FieldOffset(0)]
    private readonly byte _ms1;
    [FieldOffset(1)]
    private readonly byte _ms2;

    [FieldOffset(0)]
    private readonly ushort _ms;

    public CP16Time2a(ushort durable)
    {
        if (durable > 59999)
            throw new ArgumentOutOfRangeException(nameof(durable));
        _ms = durable;
    }

    public ushort Durable => _ms;
}
