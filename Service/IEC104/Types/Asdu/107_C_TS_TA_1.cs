using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.C_TS_TA_1, SQ.Single,
    toServerCauseOfTransmits: [COT.ACTIVATE],
    toClientCauseOfTransmits: [COT.ACTIVATE_CONFIRMATION,
    COT.UNKNOWN_TYPE_ID,
    COT.UNKNOWN_TRANSFER_REASON,
    COT.UNKNOWN_COMMON_ASDU_ADDRESS,
    COT.UNKNOWN_INFORMATION_OBJECT_ADDRESS])]
public readonly struct C_TS_TA_1
{
    public static byte Size => (byte)Marshal.SizeOf<C_TS_TA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly ushort _tsc;
    [FieldOffset(5)]
    private readonly CP56Time2a _dateTime;

    public C_TS_TA_1(ushort tsc, DateTime dateTime, TimeStatus timeStatus)
    {
        _address3 = new Address3(0);
        _tsc = tsc;
        _dateTime = new CP56Time2a(dateTime, timeStatus);
    }

    public ushort Address => _address3.Address;

    public ushort TSC => _tsc;

    public DateTime DateTime => _dateTime.DateTime;
    public TimeStatus Status => _dateTime.TimeStatus;

    public static string Description => Properties.Resources._107_C_TS_TA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var value = MemoryMarshal.AsRef<C_TS_TA_1>(buffer[..Size]);
        notification.Notify_C_TS_TA(in header, value.Address, value.TSC, value.DateTime, value.Status);
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in C_TS_TA_1 C_TS_TA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        C_TS_TA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}

