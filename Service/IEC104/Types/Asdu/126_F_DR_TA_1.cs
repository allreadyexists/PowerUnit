//using PowerUnit.Common.StructHelpers;

//using System.Runtime.InteropServices;

//namespace PowerUnit.Service.IEC104.Types.Asdu;

//[Flags]
//public enum SOF : byte
//{
//    LastFileInDirectory = 1 << 5,
//    IsSubDirectory = 1 << 6,
//    IsActiveTransmit = 1 << 7
//}

//[StructLayout(LayoutKind.Explicit, Pack = 1)]
//[AsduTypeInfo(AsduType.F_DR_TA_1, SQ.Sequence,
//    toServerCauseOfTransmits: [3, 5],
//    toClientCauseOfTransmits: [3, 5])]
//public readonly struct F_DR_TA_1_Sequence
//{
//    public static byte Size => (byte)Marshal.SizeOf<F_DR_TA_1_Sequence>();
//    public static byte MaxItemCount => (byte)((LengthHelper.PACKET_CONST_PART - 3) / Size);

//    [FieldOffset(0)]
//    private readonly ushort _nodf;
//    [FieldOffset(2)]
//    private readonly FileLength _lof;
//    [FieldOffset(5)]
//    private readonly byte _sof;
//    [FieldOffset(6)]
//    private readonly CP56Time2a _dateTime;

//    public F_DR_TA_1_Sequence(ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus = TimeStatus.OK)
//    {
//        _nodf = nodf;
//        _lof = new FileLength(lof);
//        _sof = (byte)sof;
//        _dateTime = new CP56Time2a(dateTime, timeStatus);
//    }

//    public ushort NODF => _nodf;

//    public uint LOF => _lof.Value;

//    public SOF SOF => (SOF)_sof;

//    public DateTime DateTime => _dateTime.DateTime;

//    public TimeStatus TimeStatus => _dateTime.TimeStatus;

//    public static string Description => Properties.Resources._126_F_DR_TA_1_Desc;

//    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
//    {
//        var address1 = MemoryMarshal.AsRef<Address3>(buffer[..Address3.Size]);
//        for (var i = 0; i < header.Count; i++)
//        {
//            var value = MemoryMarshal.AsRef<F_DR_TA_1_Sequence>(buffer);
//            notification.Notify_F_DR_TA(in header, address1.Address, value.NODF, value.LOF, value.SOF, value.DateTime, value.TimeStatus);
//        }
//    }

//    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, Address3 address, F_DR_TA_1_Sequence[] F_DR_TA_1_Sequences)
//    {
//        header.SerializeUnsafe(buffer, 0);
//        address.SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size);
//        var size = Address3.Size;
//        for (byte i = 0; i < F_DR_TA_1_Sequences.Length; i++)
//        {
//            F_DR_TA_1_Sequences[i].SerializeUnsafe(buffer, AsduPacketHeader_2_2.Size + Address3.Size + i * Size);
//            size += Size;
//        }

//        return AsduPacketHeader_2_2.Size + size;
//    }
//}
