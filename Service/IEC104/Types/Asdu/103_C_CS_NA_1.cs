using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.C_CS_NA_1, SQ.Single,
    toServerCauseOfTransmits: [6],
    toClientCauseOfTransmits: [3, 7, 44, 45, 46, 47])]
public readonly struct C_CS_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<C_CS_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;

    [FieldOffset(3)]
    private readonly CP56Time2a _dateTime;

    public C_CS_NA_1(DateTime dateTime, TimeStatus timeStatus)
    {
        _address3 = new Address3(0);
        _dateTime = new CP56Time2a(dateTime, timeStatus);
    }

    public ushort Address => _address3.Address;

    public DateTime DateTime => _dateTime.DateTime;

    public TimeStatus TimeStatus => _dateTime.TimeStatus;

    public static string Description => Properties.Resources._103_C_CS_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var dateTimeFromPacket = MemoryMarshal.AsRef<C_CS_NA_1>(buffer[..Size]);
        notification.Notify_C_CS_NA(in header, dateTimeFromPacket.Address, dateTimeFromPacket.DateTime, dateTimeFromPacket.TimeStatus);
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in C_CS_NA_1 C_CS_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        C_CS_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}

