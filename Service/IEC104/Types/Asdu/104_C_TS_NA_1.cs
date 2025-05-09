using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.C_TS_NA_1, SQ.Single,
    toServerCauseOfTransmits: [COT.ACTIVATE],
    toClientCauseOfTransmits: [COT.ACTIVATE_CONFIRMATION,
    COT.UNKNOWN_TYPE_ID,
    COT.UNKNOWN_TRANSFER_REASON,
    COT.UNKNOWN_COMMON_ASDU_ADDRESS,
    COT.UNKNOWN_INFORMATION_OBJECT_ADDRESS])]
public readonly struct C_TS_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<C_TS_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly ushort _fbp;

    public C_TS_NA_1(ushort fbp = 0x55AA)
    {
        _address3 = new Address3(0);
        _fbp = fbp;
    }

    public ushort Address => _address3.Address;

    public ushort FBP => _fbp;

    public static string Description => Properties.Resources._104_C_TS_NA_1_Desc;

    public static void Parse(Span<byte> buffer, ref AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var value = MemoryMarshal.AsRef<C_TS_NA_1>(buffer[..Size]);
        notification.Notify_C_TS_NA(ref header, value.Address, value.FBP);
    }

    public static int Serialize(byte[] buffer, ref AsduPacketHeader_2_2 header, ref C_TS_NA_1 C_TS_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        C_TS_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}

