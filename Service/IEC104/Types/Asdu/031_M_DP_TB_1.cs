using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_DP_TB_1, SQ.Single,
    toClientCauseOfTransmits: [3, 5, 11, 12])]
public readonly struct M_DP_TB_1_Single
{
    public static byte Size => (byte)Marshal.SizeOf<M_DP_TB_1_Single>();
    public static byte MaxItemCount => (byte)(LengthHelper.PACKET_CONST_PART / Size);

    [FieldOffset(0)]
    private readonly Address3 _address;
    [FieldOffset(3)]
    private readonly byte _diq;
    [FieldOffset(4)]
    private readonly CP56Time2a _dateTime;

    public M_DP_TB_1_Single(ushort address, DIQ_Value value, DIQ_Status diq, DateTime dateTime, TimeStatus timeStatus)
    {
        _address = new Address3(address);
        _diq = (byte)((byte)diq | (byte)value);
        _dateTime = new CP56Time2a(dateTime, timeStatus);
    }

    public ushort Address => _address.Address;
    public DIQ_Value Value => (DIQ_Value)(_diq & 0b00000011);
    public DIQ_Status Status => (DIQ_Status)(_diq & 0b11111100);
    public DateTime DateTime => _dateTime.DateTime;
    public TimeStatus TimeStatus => _dateTime.TimeStatus;

    public static string Description => Properties.Resources._031_M_DP_TB_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        for (ushort i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_DP_TB_1_Single>(buffer.Slice(i * Size, Size));
            notification.Notify_M_DP(in header, value.Address, value.Value, value.Status, value.DateTime, value.TimeStatus);
        }
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_DP_TB_1_Single M_DP_TB_1_Single)
    {
        header.SerializeUnsafe(buffer, 0);
        M_DP_TB_1_Single.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_DP_TB_1_Single[] M_DP_TB_1_Singles)
    {
        header.SerializeUnsafe(buffer, 0);
        var size = 0;
        for (byte i = 0; i < M_DP_TB_1_Singles.Length; i++)
        {
            M_DP_TB_1_Singles[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + i * Size);
            size += Size;
        }

        return AsduPacketHeader_2_2.Size + size;
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_DP_TB_1_Single[] M_DP_TB_1_Singles, byte length)
    {
        header.SerializeUnsafe(buffer, 0);
        var size = 0;
        for (byte i = 0; i < Math.Min(M_DP_TB_1_Singles.Length, length); i++)
        {
            M_DP_TB_1_Singles[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + i * Size);
            size += Size;
        }

        return AsduPacketHeader_2_2.Size + size;
    }
}

