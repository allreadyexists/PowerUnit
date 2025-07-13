//using System.Runtime.InteropServices;

//namespace PowerUnit.Service.IEC104.Types.Asdu;

//[StructLayout(LayoutKind.Explicit, Pack = 1)]
//[AsduTypeInfo(AsduType.M_DP_NA_1, SQ.Single,
//    toClientCauseOfTransmits: [2, 3, 5, 11, 12, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36]
//)]
//public readonly struct M_DP_NA_1_Single
//{
//    public static byte Size => (byte)Marshal.SizeOf<M_DP_NA_1_Single>();

//    [FieldOffset(0)]
//    private readonly Address3 _address3;
//    [FieldOffset(3)]
//    private readonly byte _diq;

//    public M_DP_NA_1_Single(ushort address, DIQ_Value value, DIQ_Status diq)
//    {
//        _address3 = new Address3(address);
//        _diq = (byte)((byte)diq | (byte)value);
//    }

//    public ushort Address => _address3.Address;

//    public DIQ_Value Value => (DIQ_Value)(_diq & 0b00000011);

//    public DIQ_Status Status => (DIQ_Status)(_diq & 0b11111100);

//    public static string Description => Properties.Resources._003_M_DP_NA_1_Desc;

//    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
//    {
//        for (ushort i = 0; i < header.Count; i++)
//        {
//            var value = MemoryMarshal.AsRef<M_DP_NA_1_Single>(buffer.Slice(i * Size, Size));
//            notification.Notify_M_DP(in header, value.Address, value.Value, value.Status, dateTime, 0);
//        }
//    }

//    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in M_DP_NA_1_Single M_DP_NA_1_Single)
//    {
//        throw new NotImplementedException();
//    }
//}

//[StructLayout(LayoutKind.Explicit, Pack = 1)]
//[AsduTypeInfo(AsduType.M_DP_NA_1, SQ.Sequence,
//    toClientCauseOfTransmits: [2, 3, 5, 11, 12, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36]
//)]
//public readonly struct M_DP_NA_1_Sequence
//{
//    public static byte Size => (byte)Marshal.SizeOf<M_DP_NA_1_Single>();

//    [FieldOffset(0)]
//    private readonly byte _diq;

//    public M_DP_NA_1_Sequence(DIQ_Value value, DIQ_Status diq)
//    {
//        _diq = (byte)((byte)diq | (byte)value);
//    }

//    public DIQ_Value Value => (DIQ_Value)(_diq & 0b00000011);

//    public DIQ_Status Status => (DIQ_Status)(_diq & 0b11111100);

//    public static string Description => Properties.Resources._003_M_DP_NA_1_Desc;

//    public static void Parse(Span<byte> buffer, in AsduPacketHeader_2_2 header, DateTime dateTime, IAsduNotification notification)
//    {
//        var address1 = MemoryMarshal.AsRef<Address3>(buffer[..Address3.Size]);
//        for (var i = 0; i < header.Count; i++)
//        {
//            var value = MemoryMarshal.AsRef<M_DP_NA_1_Sequence>(buffer.Slice(Address3.Size + i * Size, Size));
//            notification.Notify_M_DP(in header, (ushort)(address1.Address + i), value.Value, value.Status, dateTime, 0);
//        }
//    }

//    public static int Serialize(byte[] buffer, in AsduPacketHeader_2_2 header, in M_DP_NA_1_Sequence M_DP_NA_1_Sequence)
//    {
//        throw new NotImplementedException();
//    }
//}

