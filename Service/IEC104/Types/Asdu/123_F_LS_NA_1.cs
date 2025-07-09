using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.F_LS_NA_1, SQ.Single,
    toServerCauseOfTransmits: [13, 44, 45, 46, 47],
    toClientCauseOfTransmits: [13, 44, 45, 46, 47])]
public readonly struct F_LS_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<F_LS_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly ushort _nof;
    [FieldOffset(5)]
    private readonly byte _nos;
    [FieldOffset(6)]
    private readonly byte _lsq;
    [FieldOffset(7)]
    private readonly byte _chs;

    public F_LS_NA_1(ushort address, ushort nof, byte nos, LSQ lsq, byte chs)
    {
        _address3 = new Address3(address);
        _nof = nof;
        _nos = nos;
        _lsq = (byte)lsq;
        _chs = chs;
    }

    public ushort Address => _address3.Address;

    public ushort NOF => _nof;

    public byte NOS => _nos;

    public LSQ LSQ => (LSQ)_lsq;

    public byte CHS => _chs;

    public static string Description => Properties.Resources._123_F_LS_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var value = MemoryMarshal.AsRef<F_LS_NA_1>(buffer);
        notification.Notify_F_LS_NA(in header, value.Address, value.NOF, value.NOS, value.LSQ, value.CHS);
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in F_LS_NA_1 F_LS_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        F_LS_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}
