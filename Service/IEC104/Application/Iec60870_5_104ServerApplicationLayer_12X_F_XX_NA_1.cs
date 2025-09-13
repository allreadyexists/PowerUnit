using PowerUnit.Common.StringHelpers;
using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    /// <summary>
    /// Готовность файла F_FR_NA_1 = 120,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="lof"></param>
    /// <param name="frq"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_FR_NA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, uint lof, FRQ frq, CancellationToken ct)
    {
        throw new IEC60870_5_104ApplicationException(header, $"address {address} nof {nof} lof {lof} frq {frq}");
    }

    /// <summary>
    /// Готовность секции F_SR_NA_1 = 121,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="los"></param>
    /// <param name="srq"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_SR_NA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, uint los, SRQ srq, CancellationToken ct)
    {
        throw new IEC60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} los {los} srq {srq}");
    }

    /// <summary>
    /// Вызов директории, выбор файла, вызов файла, вызов секции F_SC_NA_1 = 122,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="scq"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_SC_NA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, SCQ scq, CancellationToken ct)
    {
        throw new IEC60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} scq {scq}");
    }

    /// <summary>
    /// Последняя секция, последний сегмент F_LS_NA_1 = 123,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="lsq"></param>
    /// <param name="chs"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_LS_NA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, LSQ lsq, byte chs, CancellationToken ct)
    {
        throw new IEC60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} lsq {lsq} chs {chs}");
    }

    /// <summary>
    /// Подтверждение приема файла, подтверждение приема секции F_AF_NA_1 = 124,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="afq"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_AF_NA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, AFQ afq, CancellationToken ct)
    {
        throw new IEC60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} afq {afq}");
    }

    /// <summary>
    /// Сегмент F_SG_NA_1 = 125,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="segment"></param>
    /// <param name="ct"></param>
    internal void Process_F_SG_NA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, Span<byte> segment, CancellationToken ct)
    {
        throw new IEC60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} segment {segment.ToHex()}");
    }

    /// <summary>
    /// Директория F_DR_TA_1 = 126
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nodf"></param>
    /// <param name="lof"></param>
    /// <param name="sof"></param>
    /// <param name="dateTime"></param>
    /// <param name="timeStatus"></param>
    /// <param name="ct"></param>
    internal void Process_F_DR_TA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus, CancellationToken ct)
    {
        throw new IEC60870_5_104ApplicationException(header, $"address {address} nodf {nodf} lof {lof} sof {sof} dateTime {dateTime} timeStatus {timeStatus}");
    }
}

