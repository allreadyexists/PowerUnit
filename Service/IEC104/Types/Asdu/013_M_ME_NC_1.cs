using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_ME_NC_1, SQ.Single,
    toClientCauseOfTransmits: [1, 2, 3, 5, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36])]
public readonly struct M_ME_NC_1_Single
{
    public static byte Size => (byte)Marshal.SizeOf<M_ME_NC_1_Single>();

    [FieldOffset(0)]
    private readonly Address3 _address;
    [FieldOffset(3)]
    private readonly float _value;
    [FieldOffset(7)]
    private readonly QDS_Status _qds;

    public M_ME_NC_1_Single(ushort address, float value, QDS_Status qds)
    {
        _address = new Address3(address);
        _value = value;
        _qds = qds;
    }

    public ushort Address => _address.Address;

    public float Value => _value;

    public QDS_Status Status => _qds;

    public static string Description => Properties.Resources._013_M_ME_NC_1_Desc;

    public static void Parse(Span<byte> buffer, ref AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        for (ushort i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_ME_NC_1_Single>(buffer.Slice(i * Size, Size));
            notification.Notify_M_ME(ref header, value.Address, value.Value, value.Status, dateTime, 0);
        }
    }

    public static int Serialize(byte[] buffer, ref AsduPacketHeader_2_2 header, M_ME_NC_1_Single[] M_ME_NC_1_Singles)
    {
        header.SerializeUnsafe(buffer, 0);
        var size = 0;
        for (byte i = 0; i < M_ME_NC_1_Singles.Length; i++)
        {
            M_ME_NC_1_Singles[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + i * Size);
            size += Size;
        }

        return AsduPacketHeader_2_2.Size + size;
    }
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_ME_NC_1, SQ.Sequence,
    toClientCauseOfTransmits: [1, 2, 3, 5, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36])]
public readonly struct M_ME_NC_1_Sequence
{
    public static byte Size => (byte)Marshal.SizeOf<M_ME_NC_1_Sequence>();

    [FieldOffset(0)]
    private readonly float _value;
    [FieldOffset(4)]
    private readonly QDS_Status _qds;

    public M_ME_NC_1_Sequence(float value, QDS_Status qds)
    {
        _value = value;
        _qds = qds;
    }

    public float Value => _value;

    public QDS_Status Status => _qds;

    public static string Description => Properties.Resources._013_M_ME_NC_1_Desc;

    public static void Parse(Span<byte> buffer, ref AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var address1 = MemoryMarshal.AsRef<Address3>(buffer[..Address3.Size]);
        for (var i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_ME_NC_1_Sequence>(buffer.Slice(Address3.Size + i * Size, Size));
            notification.Notify_M_ME(ref header, (ushort)(address1.Address + i), value.Value, value.Status, dateTime, 0);
        }
    }

    public static int Serialize(byte[] buffer, ref AsduPacketHeader_2_2 header, M_ME_NC_1_Sequence[] M_ME_NC_1_Singles)
    {
        throw new NotImplementedException();
    }
}

