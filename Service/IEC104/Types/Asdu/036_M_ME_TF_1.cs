using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_ME_TF_1, SQ.Single,
    toClientCauseOfTransmits: [1, 2, 3, 5, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36])]
public readonly struct M_ME_TF_1_Single
{
    public static byte Size => (byte)Marshal.SizeOf<M_ME_TF_1_Single>();
    public static byte MaxItemCount => (byte)(LengthHelper.PACKET_CONST_PART / Size);

    [FieldOffset(0)]
    private readonly Address3 _address;
    [FieldOffset(3)]
    private readonly float _value;
    [FieldOffset(7)]
    private readonly QDS_Status _qds;
    [FieldOffset(8)]
    private readonly CP56Time2a _dateTime;

    public M_ME_TF_1_Single(ushort address, float value, QDS_Status qds, DateTime dateTime, TimeStatus timeStatus)
    {
        _address = new Address3(address);
        _value = value;
        _qds = qds;
        _dateTime = new CP56Time2a(dateTime, timeStatus);
    }

    public ushort Address => _address.Address;
    public float Value => _value;
    public QDS_Status Status => _qds;
    public DateTime DateTime => _dateTime.DateTime;
    public TimeStatus TimeStatus => _dateTime.TimeStatus;

    public static string Description => Properties.Resources._036_M_ME_TF_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        for (ushort i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_ME_TF_1_Single>(buffer.Slice(i * Size, Size));
            notification.Notify_M_ME(in header, value.Address, value.Value, value.Status, value.DateTime, value.TimeStatus);
        }
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_ME_TF_1_Single M_ME_TF_1_Single)
    {
        header.SerializeUnsafe(buffer, 0);
        M_ME_TF_1_Single.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_ME_TF_1_Single[] M_ME_TF_1_Singles)
    {
        header.SerializeUnsafe(buffer, 0);
        var size = 0;
        for (byte i = 0; i < M_ME_TF_1_Singles.Length; i++)
        {
            M_ME_TF_1_Singles[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + i * Size);
            size += Size;
        }

        return AsduPacketHeader_2_2.Size + size;
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, M_ME_TF_1_Single[] M_ME_TF_1_Singles, byte length)
    {
        header.SerializeUnsafe(buffer, 0);
        var size = 0;
        for (byte i = 0; i < Math.Min(M_ME_TF_1_Singles.Length, length); i++)
        {
            M_ME_TF_1_Singles[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + i * Size);
            size += Size;
        }

        return AsduPacketHeader_2_2.Size + size;
    }
}

