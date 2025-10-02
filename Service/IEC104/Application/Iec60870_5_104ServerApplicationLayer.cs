using Microsoft.Extensions.Logging;

using PowerUnit.Common.Exceptions;
using PowerUnit.Common.StringHelpers;
using PowerUnit.Common.StructHelpers;
using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Models;
using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

using System.Runtime.CompilerServices;

using static PowerUnit.Common.StructHelpers.StructHelper;

namespace PowerUnit.Service.IEC104.Application;

public sealed partial class IEC60870_5_104ServerApplicationLayer : IASDUNotification
{
    private readonly TimeProvider _timeProvider;

    private readonly ApplicationLayerReadTransactionManager _readTransactionManager;
    private readonly string _serverName;
    private readonly IEC104ApplicationLayerModel _applicationLayerOption;

    private readonly IDataSource<MapValueItem> _dataSource;
    private readonly IDataProvider _dataProvider;

    private readonly IPhysicalLayerCommander _physicalLayerCommander;

    private readonly IIEC60870_5_104ApplicationLayerDiagnostic _diagnostic;

    private readonly ILogger<IEC60870_5_104ServerApplicationLayer> _logger;

    private readonly IChannelLayerPacketSender _packetSender;

    private readonly CancellationTokenSource _cts;

    private static readonly int _bufferizationSize = 128;
    private static readonly TimeSpan _bufferizationTimeout = TimeSpan.FromSeconds(1);
    private IDisposable? _subscriber2;

    [ThreadStatic]
    private static byte[] _sendBuffer;
    internal static byte[] SendBuffer
    {
        get
        {
            if (_sendBuffer == null)
            {
                _sendBuffer = new byte[256];
                Array.Fill<byte>(_sendBuffer, 0);
                return _sendBuffer;
            }

            return _sendBuffer;
        }
    }

    internal static void SendInRentBuffer<T>(Action<byte[], IEC60870_5_104ServerApplicationLayer, T> action, IEC60870_5_104ServerApplicationLayer context, T additionInfo)
    {
        try
        {
            action(SendBuffer, context, additionInfo);
        }
        catch (IEC60870_5_104ApplicationException iec104ApplicationException)
        {
            context._logger.LogError(iec104ApplicationException, "IEC60870_5_104ServerApplicationLayer");

            var header = iec104ApplicationException.Header;
            var headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
            COT.UNKNOWN_TRANSFER_REASON,
            pn: PN.Negative, tn: header.TN, initAddr: header.InitAddr, commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
            headerReq.SerializeUnsafe(SendBuffer, 0);
            context._packetSender!.Send(SendBuffer.AsSpan(0, ASDUPacketHeader_2_2.Size));
        }
        catch (Exception ex)
        {
            context._logger.LogError(ex, "Disconnect by exception");
            context._physicalLayerCommander.DisconnectLayer();
        }
    }

    [ThreadStatic]
    private static ASDUPacketHeader_2_2Class _header;
    internal static ASDUPacketHeader_2_2Class Header
    {
        get
        {
            if (_header == null)
            {
                _header = new ASDUPacketHeader_2_2Class();
                return _header;
            }

            return _header;
        }
    }

    private unsafe void SendValuesBase(byte[] buffer, byte initAddr, COT cot, IList<MapValueItem> values, delegate*<IEC60870_5_104ServerApplicationLayer, byte[], int, COT, void> sendActionPtr)
#pragma warning restore IDE0051 // Remove unused private members
    {
        if (values.Count > 0)
        {
            var i = 0;
            var maxPacketItemCount = 0;
            var size = 0;
            byte packetItemCount = 0;
            MapValueItem currentValue;
            TimeSpan duration;

            Header.CauseOfTransmit = (byte)((byte)PN.Positive | (byte)TN.NotTest | (byte)cot);
            Header.InitAddr = initAddr;
            Header.CommonAddrAsdu = _applicationLayerOption.CommonASDUAddress;

            while (i < values.Count)
            {
                packetItemCount = 0;

                var start = _timeProvider.GetTimestamp();

                fixed (byte* ptr = &buffer[0])
                {
                    do
                    {
                        currentValue = values[i];

                        switch (currentValue.Type)
                        {
                            case ASDUType.M_SP_TB_1:
                                ZeroCopySerialize(currentValue, ptr, ASDUPacketHeader_2_2.Size + packetItemCount * M_SP_TB_1_Single.Size, _mapValueItemToM_SP_TB_1_SingleConverterPtr);
                                size = M_SP_TB_1_Single.Size;
                                maxPacketItemCount = M_SP_TB_1_Single.MaxItemCount;
                                break;
                            case ASDUType.M_DP_TB_1:
                                ZeroCopySerialize(currentValue, ptr, ASDUPacketHeader_2_2.Size + packetItemCount * M_DP_TB_1_Single.Size, _mapValueItemToM_DP_TB_1_SingleConverterPtr);
                                size = M_DP_TB_1_Single.Size;
                                maxPacketItemCount = M_DP_TB_1_Single.MaxItemCount;
                                break;
                            case ASDUType.M_ME_TF_1:
                                ZeroCopySerialize(currentValue, ptr, ASDUPacketHeader_2_2.Size + packetItemCount * M_ME_TF_1_Single.Size, _mapValueItemToM_ME_TF_1_SingleConverterPtr);
                                size = M_ME_TF_1_Single.Size;
                                maxPacketItemCount = M_ME_TF_1_Single.MaxItemCount;
                                break;
                        }

                        packetItemCount++;
                        i++;

                        if (i == values.Count)
                            break;
                        if (packetItemCount == maxPacketItemCount)
                            break;
                        if (currentValue.Type != values[i].Type)
                            break;
                    } while (true);

                    Header.AsduType = (byte)currentValue.Type;
                    Header.VarStructInfo = (byte)((byte)SQ.Single | packetItemCount);

                    ZeroCopySerialize(Header, ptr, 0, _mapHeaderConverterPtr);
                }

                duration = _timeProvider.GetElapsedTime(start);
                _diagnostic.AppSendMsgPrepareDuration(_serverName, duration.TotalNanoseconds);

                sendActionPtr(this, buffer, ASDUPacketHeader_2_2.Size + packetItemCount * size, cot);
            }
        }
    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//#pragma warning disable IDE0051 // Remove unused private members
//    private void SendValues2(byte[] buffer, byte initAddr, COT cot, IList<MapValueItem> values)
//#pragma warning restore IDE0051 // Remove unused private members
//    {
//        SendValuesBase(buffer, initAddr, cot, values, static (ctx, buf, len, cot) =>
//        {
//            ctx._packetSender!.Send(buf.AsSpan(0, len), cot == COT.SPORADIC ? ChannelLayerPacketPriority.Low : ChannelLayerPacketPriority.Normal);
//        });
//    }

    private static readonly unsafe delegate*<IEC60870_5_104ServerApplicationLayer, byte[], int, COT, void> _sendActionPtr = &SendAction;

    private static void SendAction(IEC60870_5_104ServerApplicationLayer ctx, byte[] buf, int len, COT cot)
    {
        ctx._packetSender!.Send(buf[..len], cot == COT.SPORADIC ? ChannelLayerPacketPriority.Low : ChannelLayerPacketPriority.Normal);
    }

#pragma warning disable IDE0051 // Remove unused private members
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void SendValues(byte[] buffer, byte initAddr, COT cot, IList<MapValueItem> values)
#pragma warning restore IDE0051 // Remove unused private members
    {
        SendValuesBase(buffer, initAddr, cot, values, _sendActionPtr);
    }

    private static readonly unsafe delegate*<MapValueItem, M_SP_TB_1_SingleTemplate*, void> _mapValueItemToM_SP_TB_1_SingleConverterPtr = &MapValueItemToM_SP_TB_1_SingleConverter;

    private static unsafe void MapValueItemToM_SP_TB_1_SingleConverter(MapValueItem @object, M_SP_TB_1_SingleTemplate* @struct)
    {
        var dateTime = @object.Value.ValueDt;
        var dayOfWeek = (byte)dateTime.DayOfWeek;
        @struct->Address.Address = @object.Address;
        @struct->Address.InitAddress = 0;

        @struct->DateTime.Ms = (ushort)(dateTime.Millisecond + 1000 * dateTime.Second);
        @struct->DateTime.T3 = (byte)(dateTime.Minute & 0b00111111);
        @struct->DateTime.T4 = (byte)(dateTime.Hour & 0b00011111);
        @struct->DateTime.T5 = (byte)((byte)dateTime.Day | (byte)((dayOfWeek == 0 ? 7 : dayOfWeek) << 5));
        @struct->DateTime.T6 = (byte)(dateTime.Month & 0b00011111);
        @struct->DateTime.T7 = (byte)(dateTime.Year - 2000 & 0b01111111);

        @struct->SIQ = (byte)(@object.Value.ValueAsBool ? SIQ_Value.On : SIQ_Value.Off);
    }

    private static readonly unsafe delegate*<MapValueItem, M_DP_TB_1_SingleTemplate*, void> _mapValueItemToM_DP_TB_1_SingleConverterPtr = &MapValueItemToM_DP_TB_1_SingleConverter;

    private static unsafe void MapValueItemToM_DP_TB_1_SingleConverter(MapValueItem @object, M_DP_TB_1_SingleTemplate* @struct)
    {
        var dateTime = @object.Value.ValueDt;
        var dayOfWeek = (byte)dateTime.DayOfWeek;
        @struct->Address.Address = @object.Address;
        @struct->Address.InitAddress = 0;

        @struct->DateTime.Ms = (ushort)(dateTime.Millisecond + 1000 * dateTime.Second);
        @struct->DateTime.T3 = (byte)(dateTime.Minute & 0b00111111);
        @struct->DateTime.T4 = (byte)(dateTime.Hour & 0b00011111);
        @struct->DateTime.T5 = (byte)((byte)dateTime.Day | (byte)((dayOfWeek == 0 ? 7 : dayOfWeek) << 5));
        @struct->DateTime.T6 = (byte)(dateTime.Month & 0b00011111);
        @struct->DateTime.T7 = (byte)(dateTime.Year - 2000 & 0b01111111);

        @struct->DIQ = (byte)(@object.Value.ValueAsBool ? DIQ_Value.On : DIQ_Value.Off);
    }

    private static readonly unsafe delegate*<MapValueItem, M_ME_TF_1_SingleTemplate*, void> _mapValueItemToM_ME_TF_1_SingleConverterPtr = &MapValueItemToM_ME_TF_1_SingleConverter;

    private static unsafe void MapValueItemToM_ME_TF_1_SingleConverter(MapValueItem @object, M_ME_TF_1_SingleTemplate* @struct)
    {
        var dateTime = @object.Value.ValueDt;
        var dayOfWeek = (byte)dateTime.DayOfWeek;
        @struct->Address.Address = @object.Address;
        @struct->Address.InitAddress = 0;

        @struct->DateTime.Ms = (ushort)(dateTime.Millisecond + 1000 * dateTime.Second);
        @struct->DateTime.T3 = (byte)(dateTime.Minute & 0b00111111);
        @struct->DateTime.T4 = (byte)(dateTime.Hour & 0b00011111);
        @struct->DateTime.T5 = (byte)((byte)dateTime.Day | (byte)((dayOfWeek == 0 ? 7 : dayOfWeek) << 5));
        @struct->DateTime.T6 = (byte)(dateTime.Month & 0b00011111);
        @struct->DateTime.T7 = (byte)(dateTime.Year - 2000 & 0b01111111);

        @struct->QDS = 0;
        @struct->Value = @object.Value.ValueAsFloat;
    }

    private static readonly unsafe delegate*<ASDUPacketHeader_2_2Class, ASDUPacketHeader_2_2Template*, void> _mapHeaderConverterPtr = &MapHeaderConverter;

    private static unsafe void MapHeaderConverter(ASDUPacketHeader_2_2Class @object, ASDUPacketHeader_2_2Template* @struct)
    {
        @struct->AsduType = @object.AsduType;
        @struct->VarStructInfo = @object.VarStructInfo;
        @struct->CauseOfTransmit = @object.CauseOfTransmit;
        @struct->InitAddr = @object.InitAddr;
        @struct->CommonAddrAsdu = @object.CommonAddrAsdu;
    }

    public IEC60870_5_104ServerApplicationLayer(IEC104ServerModel serverModel,
        IDataSource<MapValueItem> dataSource,
        IDataProvider dataProvider,
        IChannelLayerPacketSender packetSender,
        IPhysicalLayerCommander physicalLayerCommander,
        TimeProvider timeProvider,
        IIEC60870_5_104ApplicationLayerDiagnostic diagnostic,
        ILogger<IEC60870_5_104ServerApplicationLayer> logger)
    {
        _timeProvider = timeProvider;
        _readTransactionManager = new ApplicationLayerReadTransactionManager();
        _serverName = serverModel.ServerName;
        _applicationLayerOption = serverModel.ApplicationLayerModel;

        _dataSource = dataSource;
        _dataProvider = dataProvider;
        _packetSender = packetSender;
        _physicalLayerCommander = physicalLayerCommander;
        _diagnostic = diagnostic;
        _cts = new CancellationTokenSource();

        _logger = logger;

        SendInRentBuffer(static (buffer, context, additionInfo) =>
        {
            var headerReq = new ASDUPacketHeader_2_2(ASDUType.M_EI_NA_1, SQ.Single, 1,
            COT.INIT_MESSAGE,
            initAddr: 0, commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
            var M_EI_NA_1 = new M_EI_NA_1(COI.Empty);
            var length = M_EI_NA_1.Serialize(buffer, in headerReq, in M_EI_NA_1);
            context._packetSender!.Send(buffer.AsSpan(0, length));

            if (context._applicationLayerOption.SporadicSendEnabled)
            {
                context._subscriber2 = new BatchSubscriber<MapValueItem, IEC60870_5_104ServerApplicationLayer>(
                    _bufferizationSize, _bufferizationTimeout, context._dataSource, context,
                    static (values, context2, token) =>
                    {
                        if (values.TryGetNonEnumeratedCount(out var count) && count > 0)
                        {
                            SendInRentBuffer(static (buffer, context3, additionInfo2) =>
                            {
                                context3.Stream(buffer, additionInfo2);
                            }, context2, values);
                        }

                        return Task.CompletedTask;
                    });
            }
        }, this, this);
    }

    void IASDUNotification.Notify_M_SP(in ASDUPacketHeader_2_2 header, ushort address, SIQ_Value value, SIQ_Status siq, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace("{@address} {@value} {@siq} {@dateTime} {@status}", address, value, siq, dateTime, status);
    }

    void IASDUNotification.Notify_M_DP(in ASDUPacketHeader_2_2 header, ushort address, DIQ_Value value, DIQ_Status diq, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace("{@address} {@value} {@diq} {@dateTime} {@status}", address, value, diq, dateTime, status);
    }

    void IASDUNotification.Notify_M_ME(in ASDUPacketHeader_2_2 header, ushort address, float value, QDS_Status qds, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace("{@address} {@value} {@qds} {@dateTime} {@status}", address, value, qds, dateTime, status);
    }

    void IASDUNotification.Notify_C_IC_NA(in ASDUPacketHeader_2_2 header, ushort address, QOI qoi)
    {
        _logger.LogTrace("{@address} {@qoi}", address, qoi);
        Process_C_IC_NA_1(in header, address, qoi, _cts.Token);
    }

    void IASDUNotification.Notify_C_RD_NA(in ASDUPacketHeader_2_2 header, ushort address)
    {
        _logger.LogTrace("{@address}", address);
        Process_C_RD_NA_1(in header, address, _cts.Token);
    }

    void IASDUNotification.Notify_C_CS_NA(in ASDUPacketHeader_2_2 header, ushort address, DateTime dateTime, TimeStatus timeStatus)
    {
        _logger.LogTrace("{@address} {@dateTime} {@timeStatus}", address, dateTime, timeStatus);
        Process_C_CS_NA_1(in header, address, dateTime, timeStatus, _cts.Token);
    }

    void IASDUNotification.Notify_C_CI_NA(in ASDUPacketHeader_2_2 header, ushort address, QCC qcc)
    {
        _logger.LogTrace("{@address} {@qcc}", address, qcc);
        Process_C_CI_NA_1(in header, address, qcc, _cts.Token);
    }

    void IASDUNotification.Notify_F_FR_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, uint lof, FRQ frq)
    {
        _logger.LogTrace("{@address} {@nof} {@lof} {@frq}", address, nof, lof, frq);
        Process_F_FR_NA_1(in header, address, nof, lof, frq, _cts.Token);
    }

    void IASDUNotification.Notify_F_SR_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, uint los, SRQ frq)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@los} {@frq}", address, nof, nos, los, frq);
        Process_F_SR_NA_1(in header, address, nof, nos, los, frq, _cts.Token);
    }

    void IASDUNotification.Notify_F_SC_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, SCQ scq)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@scq}", address, nof, nos, scq);
        Process_F_SC_NA_1(in header, address, nof, nos, scq, _cts.Token);
    }

    void IASDUNotification.Notify_F_LS_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, LSQ lsq, byte chs)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@lsq} {@chs}", address, nof, nos, lsq, chs);
        Process_F_LS_NA_1(in header, address, nof, nos, lsq, chs, _cts.Token);
    }

    void IASDUNotification.Notify_F_AF_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, AFQ afq)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@afq}", address, nof, nos, afq);
        Process_F_AF_NA_1(in header, address, nof, nos, afq, _cts.Token);
    }

    void IASDUNotification.Notify_F_SG_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, Span<byte> segment)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@segment.ToHex()}", address, nof, nos, segment.ToHex());
        Process_F_SG_NA_1(in header, address, nof, nos, segment, _cts.Token);
    }

    void IASDUNotification.Notify_F_DR_TA(in ASDUPacketHeader_2_2 header, ushort address, ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus)
    {
        _logger.LogTrace("{@address} {@nodf} {@lof} {@sof} {@dateTime} {@timeStatus}", address, nodf, lof, sof, dateTime, timeStatus);
        Process_F_DR_TA_1(in header, address, nodf, lof, sof, dateTime, timeStatus, _cts.Token);
    }

    void IASDUNotification.Notify_C_TS_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort fbp)
    {
        _logger.LogTrace("{@address} {@fbp}", address, fbp);
        Process_C_TS_NA_1(in header, address, fbp, _cts.Token);
    }

    void IASDUNotification.Notify_C_TS_TA(in ASDUPacketHeader_2_2 header, ushort address, ushort tsc, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace("{@address} {@tsc} {@dateTime} {@status}", address, tsc, dateTime, status);
        Process_C_TS_TA_1(in header, address, tsc, dateTime, status, _cts.Token);
    }

    void IASDUNotification.Notify_Unknown_Asdu_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        Process_Notify_Unknown_Asdu_Raw(in header, asduInfoRaw, _cts.Token);
    }

    void IASDUNotification.Notify_Unknown_Cot_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        Process_Notify_Unknown_Cot_Raw(in header, asduInfoRaw, _cts.Token);
    }

    void IASDUNotification.Notify_Unknown_Exception(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, Exception ex)
    {
        _logger.LogCritical("{@header} {@asduInfoRaw} {@ex}", header, asduInfoRaw.ToHex(), ex.GetInnerExceptionsString());
        Process_Notify_Unknown_Exception(in header, asduInfoRaw, _cts.Token);
    }

    bool IASDUNotification.Notify_CommonAsduAddress(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        return Process_Notify_CommonAsduAddress(in header, asduInfoRaw, _cts.Token);
    }

    void IDisposable.Dispose()
    {
        _subscriber2?.Dispose();
        _cts.Cancel();
        _cts.Dispose();
    }

    void IASDUNotification.Notify_M_EI_NA(in ASDUPacketHeader_2_2 header, ushort address, COI coi)
    {
    }
}

