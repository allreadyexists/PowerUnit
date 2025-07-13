using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_SP_TB_1, SQ.Single,
    toClientCauseOfTransmits: [3, 5, 11, 12])]
public readonly struct M_SP_TB_1_Single
{
    public static byte Size => (byte)Marshal.SizeOf<M_SP_TB_1_Single>();
    public static byte MaxItemCount => (byte)(LengthHelper.PACKET_CONST_PART / Size);

    [FieldOffset(0)]
    private readonly Address3 _address;
    [FieldOffset(3)]
    private readonly byte _siq;
    [FieldOffset(4)]
    private readonly CP56Time2a _dateTime;

    public M_SP_TB_1_Single(ushort address, SIQ_Value value, SIQ_Status siq, DateTime dateTime, TimeStatus timeStatus)
    {
        _address = new Address3(address);
        _siq = (byte)((byte)siq | (byte)value);
        _dateTime = new CP56Time2a(dateTime, timeStatus);
    }

    public ushort Address => _address.Address;
    public SIQ_Value Value => (SIQ_Value)(_siq & 1);

    public SIQ_Status Status => (SIQ_Status)(_siq & 0b11111110);
    public DateTime DateTime => _dateTime.DateTime;
    public TimeStatus TimeStatus => _dateTime.TimeStatus;

    public static string Description => Properties.Resources._030_M_SP_TB_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        for (ushort i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_SP_TB_1_Single>(buffer.Slice(i * Size, Size));
            notification.Notify_M_SP(in header, value.Address, value.Value, value.Status, value.DateTime, value.TimeStatus);
        }
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_SP_TB_1_Single M_SP_TB_1_Single)
    {
        header.SerializeUnsafe(buffer, 0);
        M_SP_TB_1_Single.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_SP_TB_1_Single[] M_SP_TB_1_Singles)
    {
        header.SerializeUnsafe(buffer, 0);
        var size = 0;
        for (byte i = 0; i < M_SP_TB_1_Singles.Length; i++)
        {
            M_SP_TB_1_Singles[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + i * Size);
            size += Size;
        }

        return AsduPacketHeader_2_2.Size + size;
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_SP_TB_1_Single[] M_SP_TB_1_Singles, byte length)
    {
        header.SerializeUnsafe(buffer, 0);
        var size = 0;
        for (byte i = 0; i < Math.Min(M_SP_TB_1_Singles.Length, length); i++)
        {
            M_SP_TB_1_Singles[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + i * Size);
            size += Size;
        }

        return AsduPacketHeader_2_2.Size + size;
    }
}

