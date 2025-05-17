using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_EI_NA_1, SQ.Single, toServerCauseOfTransmits: [COT.INIT_MESSAGE])]
public readonly struct M_EI_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<M_EI_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly byte _coi;

    public M_EI_NA_1(COI coi)
    {
        _address3 = new Address3(0);
        _coi = (byte)coi;
    }

    public ushort Address => _address3.Address;

    public COI COI => (COI)_coi;

    public static string Description => Properties.Resources._070_M_EI_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var coi = MemoryMarshal.AsRef<M_EI_NA_1>(buffer[..Size]);
        notification.Notify_M_EI_NA(in header, coi.Address, coi.COI);
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in M_EI_NA_1 M_EI_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        M_EI_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}

