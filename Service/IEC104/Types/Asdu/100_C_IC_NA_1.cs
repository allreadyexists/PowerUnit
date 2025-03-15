using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[ASDUTypeInfo(ASDUType.C_IC_NA_1, SQ.Single,
    toServerCauseOfTransmits: [COT.ACTIVATE,
    COT.DEACTIVATE],
    toClientCauseOfTransmits: [COT.ACTIVATE_CONFIRMATION,
    COT.DEACTIVATE_CONFIRMATION,
    COT.ACTIVATE_COMPLETION,
    COT.UNKNOWN_TYPE_ID,
    COT.UNKNOWN_TRANSFER_REASON,
    COT.UNKNOWN_COMMON_ASDU_ADDRESS,
    COT.UNKNOWN_INFORMATION_OBJECT_ADDRESS])]
public readonly struct C_IC_NA_1
{
    public static byte Size => (byte)Marshal.SizeOf<C_IC_NA_1>();

    [FieldOffset(0)]
    private readonly Address3 _address3;
    [FieldOffset(3)]
    private readonly byte _qoi;

    public C_IC_NA_1(QOI qoi)
    {
        _address3 = new Address3(0);
        _qoi = (byte)qoi;
    }

    public ushort Address => _address3.Address;

    public QOI QOI => (QOI)_qoi;

    public static string Description => Properties.Resources._100_C_IC_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in ASDUPacketHeader_2_2 header, DateTime dateTime, IASDUNotification notification)
    {
        var qoi = MemoryMarshal.AsRef<C_IC_NA_1>(buffer[..Size]);
        notification.Notify_C_IC_NA(in header, qoi.Address, qoi.QOI);
    }

    public static int Serialize(byte[] buffer, in ASDUPacketHeader_2_2 header, in C_IC_NA_1 C_IC_NA_1)
    {
        header.SerializeUnsafe(buffer, 0);
        C_IC_NA_1.SerializeUnsafe(buffer, ASDUPacketHeader_2_2.Size);
        return ASDUPacketHeader_2_2.Size + Size;
    }
}

