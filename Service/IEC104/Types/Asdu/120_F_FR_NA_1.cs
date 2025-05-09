using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.F_FR_NA_1, SQ.Single,
    toServerCauseOfTransmits: [13, 44, 45, 46, 47],
    toClientCauseOfTransmits: [13, 44, 45, 46, 47])]
public readonly struct F_FR_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<F_FR_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly ushort _nof;
    [FieldOffset(5)]
    private readonly FileLength _lof;
    [FieldOffset(8)]
    private readonly byte _frq;

    public F_FR_NA_1(ushort address, ushort nof, uint lof, FRQ frq)
    {
        _address3 = new Address3(address);
        _nof = nof;
        _lof = new FileLength(lof);
        _frq = (byte)frq;
    }

    public ushort Address => _address3.Address;

    public ushort NOF => _nof;

    public uint LOF => _lof.Value;

    public FRQ FRQ => (FRQ)_frq;

    public static string Description => Properties.Resources._120_F_FR_NA_1_Desc;

    public static void Parse(Span<byte> buffer, ref AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var value = MemoryMarshal.AsRef<F_FR_NA_1>(buffer);
        notification.Notify_F_FR_NA(ref header, value.Address, value.NOF, value.LOF, value.FRQ);
    }

    public static int Serialize(byte[] buffer, ref AsduPacketHeader_2_2 header, ref F_FR_NA_1 F_FR_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        F_FR_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}
