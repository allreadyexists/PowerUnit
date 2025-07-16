using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[ASDUTypeInfo(ASDUType.C_CI_NA_1, SQ.Single,
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

    public static void Parse(Span<byte> buffer, in ASDUPacketHeader_2_2 header, DateTime dateTime, IASDUNotification notification)
    {
        var qcc = MemoryMarshal.AsRef<C_CI_NA_1>(buffer[..Size]);
        notification.Notify_C_CI_NA(in header, qcc.Address, qcc.QCC);
    }

    public static int Serialize(byte[] buffer, in ASDUPacketHeader_2_2 header, in C_CI_NA_1 C_CI_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        C_CI_NA_1.SerializeUnsafe(buffer, ASDUPacketHeader_2_2.Size);
        return ASDUPacketHeader_2_2.Size + Size;
    }
}

