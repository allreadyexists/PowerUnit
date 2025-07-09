using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types;

/// <summary>
/// Заголовок пакета - причина передачи 2 байта, общий адрес ASDU 2 байта
/// параметры постоянные для МЭК 104
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct AsduPacketHeader_2_2
{
    public static byte Size => (byte)Marshal.SizeOf<AsduPacketHeader_2_2>();

    /// <summary>
    /// Тип ASDU
    /// </summary>
    [FieldOffset(0)]
    private readonly byte _asduType;

    #region Классификатор переменной структуры
    ///// <summary>
    ///// Одиночный - 0 индивидуальные / последовательность - 1 последовательность подряд идущих адресов
    ///// </summary>
    //public SQ SQ { get; }
    ///// <summary>
    ///// Число объектов информации
    ///// </summary>
    //public byte Count { get; }
    [FieldOffset(1)]
    private readonly byte _varStructInfo;

    #endregion

    #region Причины передачи
    //public CauseOfTransmit CauseOfTransmit { get; }
    //public PN PN { get; }
    //public TN TN { get; }
    [FieldOffset(2)]
    private readonly byte _causeOfTransmit;
    #endregion

    /// <summary>
    /// Адрес инициализатора
    /// </summary>
    [FieldOffset(3)]
    private readonly byte _initAddr;

    /// <summary>
    /// Общий адрес ASDU
    /// </summary>
    [FieldOffset(4)]
    private readonly ushort _commonAddrAsdu;

    public AsduPacketHeader_2_2(AsduType asduType, SQ sq, byte count, COT causeOfTransmit, PN pn = PN.Positive, TN tn = TN.NotTest, byte initAddr = 0, ushort commonAddrAsdu = 1)
    {
        if (count > 0x7F)
            throw new ArgumentOutOfRangeException(nameof(count));
        _asduType = (byte)asduType;
        _varStructInfo = (byte)((byte)sq | count);
        _causeOfTransmit = (byte)((byte)pn | (byte)tn | (byte)causeOfTransmit);
        _initAddr = initAddr;
        _commonAddrAsdu = commonAddrAsdu;
    }

    public AsduType AsduType => (AsduType)_asduType;

    public SQ SQ => (_varStructInfo & (byte)SQ.Sequence) != 0 ? SQ.Sequence : SQ.Single;

    public byte Count => (byte)(_varStructInfo & 0x7F);

    public COT CauseOfTransmit => (COT)(_causeOfTransmit & 0x3F);

    public PN PN => (_causeOfTransmit & (byte)PN.Negative) != 0 ? PN.Negative : PN.Positive;

    public TN TN => (_causeOfTransmit & (byte)TN.Test) != 0 ? TN.Test : TN.NotTest;

    public readonly byte InitAddr => _initAddr;

    public readonly ushort CommonAddrAsdu => _commonAddrAsdu;

    public override string ToString() => $"{AsduType} {SQ} {Count} {CauseOfTransmit} {PN} {TN} {_initAddr} {_commonAddrAsdu}";
}

