using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_DP_TA_1, SQ.Single,
    toClientCauseOfTransmits: [3, 5, 11, 12])]
public readonly struct M_DP_TA_1_Single
{
    public static byte Size => (byte)Marshal.SizeOf<M_DP_TA_1_Single>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly byte _diq;
    [FieldOffset(4)]
    private readonly CP24Time2a _cp24;

    public M_DP_TA_1_Single(ushort address, DIQ_Value value, DIQ_Status diq, TimeOnly timeOnly, TimeStatus timeStatus = 0)
    {
        _address3 = new Address3(address);
        _diq = (byte)((byte)diq | (byte)value);
        _cp24 = new CP24Time2a(timeOnly, timeStatus);
    }

    public ushort Address => _address3.Address;

    public DIQ_Value Value => (DIQ_Value)(_diq & 0b00000011);

    public DIQ_Status Status => (DIQ_Status)(_diq & 0b11111100);

    public DateTime DateTime(DateTime now) => _cp24.DateTime(now);

    public TimeStatus TimeStatus => _cp24.TimeStatus;

    public static string Description => Properties.Resources._004_M_DP_TA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        for (ushort i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_DP_TA_1_Single>(buffer.Slice(i * Size, Size));
            notification.Notify_M_DP(in header, value.Address, value.Value, value.Status, value.DateTime(dateTime), value.TimeStatus);
        }
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_DP_TA_1_Single[] M_DP_TA_1_Singles)
    {
        var length = AsduPacketHeader_2_2.Size;
        header.SerializeUnsafe(buffer, 0);
        for (var i = 0; i < header.Count; i++)
        {
            M_DP_TA_1_Singles[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + i * Size);
            length += Size;
        }

        return length;
    }
}

