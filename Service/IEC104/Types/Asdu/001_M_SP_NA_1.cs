using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types.Asdu;

/*
001 - Одноэлементная информация без метки времени
 */

/// <summary>
/// Одноэлементная информация значение, определенная в 7.2.6.1
/// </summary>
public enum SIQ_Value : byte
{
    Off = 0,
    On = 1
}

/// <summary>
/// Одноэлементная информация статус, определенная в 7.2.6.1
/// </summary>
[Flags]
public enum SIQ_Status : byte
{
    BL = 0b00010000,
    SB = 0b00100000,
    NT = 0b01000000,
    IV = 0b10000000
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_SP_NA_1, SQ.Single,
    toServerCauseOfTransmits: [],
    toClientCauseOfTransmits: [COT.BACKGRAUND_SCAN,
    COT.SPORADIC,
    COT.REQUEST_REQUESTED_DATA,
    COT.REMOTE_COMMANT_FEEDBACK,
    COT.LOCAL_COMMAND_FEEDBACK,
    COT.INTERROGATION_COMMON,
    COT.INTERROGATION_GROUP_1,
    COT.INTERROGATION_GROUP_2,
    COT.INTERROGATION_GROUP_3,
    COT.INTERROGATION_GROUP_4,
    COT.INTERROGATION_GROUP_5,
    COT.INTERROGATION_GROUP_6,
    COT.INTERROGATION_GROUP_7,
    COT.INTERROGATION_GROUP_8,
    COT.INTERROGATION_GROUP_9,
    COT.INTERROGATION_GROUP_10,
    COT.INTERROGATION_GROUP_11,
    COT.INTERROGATION_GROUP_12,
    COT.INTERROGATION_GROUP_13,
    COT.INTERROGATION_GROUP_14,
    COT.INTERROGATION_GROUP_15,
    COT.INTERROGATION_GROUP_16])]
public readonly struct M_SP_NA_1_Single
{
    public static byte Size => (byte)Marshal.SizeOf<M_SP_NA_1_Single>();

    [FieldOffset(0)]
    private readonly Address3 _address;
    [FieldOffset(3)]
    private readonly byte _siq;

    public M_SP_NA_1_Single(ushort address, SIQ_Value value, SIQ_Status siq)
    {
        _address = new Address3(address);
        _siq = (byte)((byte)siq | (byte)value);
    }

    public ushort Address => _address.Address;

    public SIQ_Value Value => (SIQ_Value)(_siq & 1);

    public SIQ_Status Status => (SIQ_Status)(_siq & 0b11111110);

    public static string Description => Properties.Resources._001_M_SP_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        for (ushort i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_SP_NA_1_Single>(buffer.Slice(i * Size, Size));
            notification.Notify_M_SP(in header, value.Address, value.Value, value.Status, dateTime, 0);
        }
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in M_SP_NA_1_Single M_SP_NA_1_Single)
    {
        throw new NotImplementedException();
    }
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
[AsduTypeInfo(AsduType.M_SP_NA_1, SQ.Sequence,
    toServerCauseOfTransmits: [],
    toClientCauseOfTransmits: [COT.BACKGRAUND_SCAN,
    COT.SPORADIC,
    COT.REQUEST_REQUESTED_DATA,
    COT.REMOTE_COMMANT_FEEDBACK,
    COT.LOCAL_COMMAND_FEEDBACK,
    COT.INTERROGATION_COMMON,
    COT.INTERROGATION_GROUP_1,
    COT.INTERROGATION_GROUP_2,
    COT.INTERROGATION_GROUP_3,
    COT.INTERROGATION_GROUP_4,
    COT.INTERROGATION_GROUP_5,
    COT.INTERROGATION_GROUP_6,
    COT.INTERROGATION_GROUP_7,
    COT.INTERROGATION_GROUP_8,
    COT.INTERROGATION_GROUP_9,
    COT.INTERROGATION_GROUP_10,
    COT.INTERROGATION_GROUP_11,
    COT.INTERROGATION_GROUP_12,
    COT.INTERROGATION_GROUP_13,
    COT.INTERROGATION_GROUP_14,
    COT.INTERROGATION_GROUP_15,
    COT.INTERROGATION_GROUP_16])]
public readonly struct M_SP_NA_1_Sequence
{
    public static byte Size => (byte)Marshal.SizeOf<M_SP_NA_1_Sequence>();

    [FieldOffset(0)]
    private readonly byte _siq;

    public M_SP_NA_1_Sequence(SIQ_Value value, SIQ_Status siq)
    {
        _siq = (byte)((byte)siq | (byte)value);
    }

    public SIQ_Value Value => (SIQ_Value)(_siq & 1);

    public SIQ_Status Status => (SIQ_Status)(_siq & 0b11111110);

    public static string Description => Properties.Resources._001_M_SP_NA_1_Desc;

    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
    {
        var address1 = MemoryMarshal.AsRef<Address3>(buffer[..Address3.Size]);
        for (var i = 0; i < header.Count; i++)
        {
            var value = MemoryMarshal.AsRef<M_SP_NA_1_Sequence>(buffer.Slice(Address3.Size + i * Size, Size));
            notification.Notify_M_SP(in header, (ushort)(address1.Address + i), value.Value, value.Status, dateTime, 0);
        }
    }

    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in M_SP_NA_1_Sequence M_SP_NA_1_Sequence)
    {
        throw new NotImplementedException();
    }
}
