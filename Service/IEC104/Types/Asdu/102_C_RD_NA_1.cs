using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.C_RD_NA_1, SQ.Single,
    toServerCauseOfTransmits: [COT.REQUEST_REQUESTED_DATA],
    toClientCauseOfTransmits: [
    COT.UNKNOWN_TYPE_ID,
    COT.UNKNOWN_TRANSFER_REASON,
    COT.UNKNOWN_COMMON_ASDU_ADDRESS,
    COT.UNKNOWN_INFORMATION_OBJECT_ADDRESS])]
public readonly struct C_RD_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<C_RD_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;

    public C_RD_NA_1(ushort address)
    {
        _address3 = new Address3(address);
    }

    public ushort Address => _address3.Address;

    public static string Description => Properties.Resources._102_C_RD_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var address = MemoryMarshal.AsRef<C_RD_NA_1>(buffer[..Size]);
        notification.Notify_C_RD_NA(in header, address.Address);
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in C_RD_NA_1 C_RD_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        C_RD_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}

