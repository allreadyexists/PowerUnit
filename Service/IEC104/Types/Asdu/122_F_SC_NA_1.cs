using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.F_SC_NA_1, SQ.Single,
    toServerCauseOfTransmits: [5, 13, 44, 45, 46, 47],
    toClientCauseOfTransmits: [5, 13, 44, 45, 46, 47])]
public readonly struct F_SC_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<F_SC_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly ushort _nof;
    [FieldOffset(5)]
    private readonly byte _nos;
    [FieldOffset(6)]
    private readonly byte _scq;

    public F_SC_NA_1(ushort address, ushort nof, byte nos, SCQ scq)
    {
        _address3 = new Address3(address);
        _nof = nof;
        _nos = nos;
        _scq = (byte)scq;
    }

    public ushort Address => _address3.Address;

    public ushort NOF => _nof;

    public byte NOS => _nos;

    public SCQ SCQ => (SCQ)_scq;

    public static string Description => Properties.Resources._122_F_SC_NA_1_Desc;

    public static void Parse(Span<byte> buffer, ref AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var value = MemoryMarshal.AsRef<F_SC_NA_1>(buffer);
        notification.Notify_F_SC_NA(ref header, value.Address, value.NOF, value.NOS, value.SCQ);
    }

    public static int Serialize(byte[] buffer, ref AsduPacketHeader_2_2 header, ref F_SC_NA_1 F_SC_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        F_SC_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}
