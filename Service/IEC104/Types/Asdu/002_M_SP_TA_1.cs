using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_SP_TA_1, SQ.Single,
    toClientCauseOfTransmits: [3, 5, 11, 12])]
public readonly struct M_SP_TA_1_Single
{
    public static byte Size => (byte)Marshal.SizeOf<M_SP_NA_1_Single>();

    [FieldOffset(0)]
    private readonly Address3 _address;
    [FieldOffset(3)]
    private readonly byte _siq;
    [FieldOffset(4)]
    private readonly CP24Time2a _cp24;

    public M_SP_TA_1_Single(ushort address, SIQ_Value value, SIQ_Status siq, TimeOnly timeOnly, TimeStatus timeStatus)
    {
        _address = new Address3(address);
        _siq = (byte)((byte)siq | (byte)value);
        _cp24 = new CP24Time2a(timeOnly, timeStatus);
    }

    public ushort Address => _address.Address;

    public SIQ_Value Value => (SIQ_Value)(_siq & 1);

    public SIQ_Status Status => (SIQ_Status)(_siq & 0b11111110);

    public static string Description => Properties.Resources._002_M_SP_TA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        for (ushort i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_SP_TA_1_Single>(buffer.Slice(i * Size, Size));
            notification.Notify_M_SP(in header,
                value.Address, value.Value, value.Status, value._cp24.DateTime(dateTime), value._cp24.TimeStatus);
        }
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in M_SP_TA_1_Single M_SP_TA_1_Single)
    {
        throw new NotImplementedException();
    }
}

