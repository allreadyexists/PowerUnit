using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[Flags]
public enum QCC : byte
{
    GROUP1 = 1,
    GROUP2 = 2,
    GROUP3 = 3,
    GROUP4 = 4,
    COMMON = 5,
    FIX = 1 << 6,
    FIX_AND_RESET = 1 << 7,
    RESET = 3 << 6
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.C_CI_NA_1, SQ.Single,
    toServerCauseOfTransmits: [6, 8],
    toClientCauseOfTransmits: [7, 9, 10, 44, 45, 46, 47])]
public readonly struct C_CI_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<C_CI_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;

    [FieldOffset(3)]
    private readonly byte _qcc;

    public C_CI_NA_1(QCC qcc)
    {
        _address3 = new Address3(0);
        _qcc = (byte)qcc;
    }

    public ushort Address => _address3.Address;

    public QCC QCC => (QCC)_qcc;

    public static string Description => Properties.Resources._101_C_CI_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var qcc = MemoryMarshal.AsRef<C_CI_NA_1>(buffer[..Size]);
        notification.Notify_C_CI_NA(in header, qcc.Address, qcc.QCC);
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in C_CI_NA_1 C_CI_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        C_CI_NA_1.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
        return AsduPacketHeader_2_2.Size + Size;
    }
}

