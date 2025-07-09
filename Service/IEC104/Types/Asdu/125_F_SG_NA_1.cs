using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.F_SG_NA_1, SQ.Single,
    toServerCauseOfTransmits: [13, 44, 45, 46, 47],
    toClientCauseOfTransmits: [13, 44, 45, 46, 47])]
public readonly struct F_SG_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<F_SG_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly ushort _nof;
    [FieldOffset(5)]
    private readonly byte _nos;
    [FieldOffset(6)]
    private readonly byte _los;

    public F_SG_NA_1(ushort address, ushort nof, byte nos, byte los)
    {
        _address3 = new Address3(address);
        _nof = nof;
        _nos = nos;
        _los = los;
    }

    public ushort Address => _address3.Address;
    public ushort NOF => _nof;
    public byte NOS => _nos;
    public byte LOS => _los;

    public static string Description => Properties.Resources._125_F_SG_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var value = MemoryMarshal.AsRef<F_SG_NA_1>(buffer);
        notification.Notify_F_SG_NA(in header, value.Address, value.NOF, value.NOS, buffer[Size..(Size + value.LOS)]);
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in F_SG_NA_1 F_SG_NA_1, byte[] segment)
    {
        header.SerializeUnsafe(buffer, 0);
        F_SG_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        segment.CopyTo(buffer, AsduPacketHeader_2_2.Size + Size);
        return AsduPacketHeader_2_2.Size + Size + segment.Length;
    }
}
