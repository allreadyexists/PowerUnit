using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Common.Exceptions;
using PowerUnit.Common.StringHelpers;
using PowerUnit.Common.StructHelpers;
using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

using System.Buffers;
using System.Reactive.Linq;

namespace PowerUnit.Service.IEC104.Application;

public sealed partial class IEC60870_5_104ServerApplicationLayer : IASDUNotification
{
    private readonly TimeProvider _timeProvider;

    private readonly ApplicationLayerReadTransactionManager _readTransactionManager;

    private readonly IEC104ApplicationLayerModel _applicationLayerOption;

    private readonly IDataProvider _dataProvider;

    private readonly IPhysicalLayerCommander _physicalLayerCommander;

    private readonly ILogger<IEC60870_5_104ServerApplicationLayer> _logger;

    private readonly IChannelLayerPacketSender _packetSender;

    private readonly CancellationTokenSource _cts;

    private static readonly int _bufferizationSize = 100;
    private static readonly TimeSpan _bufferizationTimeout = TimeSpan.FromMilliseconds(500);
    private IDisposable? _subscriber2;

    internal async Task SendInRentBuffer(Func<byte[], Task> action)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(256);
        try
        {
            await action(buffer);
        }
        catch (IEC60870_5_104ApplicationException iec104ApplicationException)
        {
            _logger.LogError(iec104ApplicationException, "IEC60870_5_104ServerApplicationLayer");

            var header = iec104ApplicationException.Header;
            var headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
            COT.UNKNOWN_TRANSFER_REASON,
            pn: PN.Negative, tn: header.TN, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
            headerReq.SerializeUnsafe(buffer, 0);
            _packetSender!.Send(buffer[..ASDUPacketHeader_2_2.Size]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Disconnect by exception");
            _physicalLayerCommander.DisconnectLayer();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void SendValues(byte[] buffer, byte initAddr, COT cot, IEnumerable<MapValueItem> values)
    {
        var M_SP_TB_1_SingleArray = ArrayPool<M_SP_TB_1_Single>.Shared.Rent(M_SP_TB_1_Single.MaxItemCount);
        var M_DP_TB_1_SingleArray = ArrayPool<M_DP_TB_1_Single>.Shared.Rent(M_DP_TB_1_Single.MaxItemCount);
        var M_ME_TF_1_SingleArray = ArrayPool<M_ME_TF_1_Single>.Shared.Rent(M_ME_TF_1_Single.MaxItemCount);

        int length = 0;
        byte count = 0;

        ASDUType currentType = 0;
        var currentTypeMaxCount = 0;

        var isInit = false;

        try
        {
            foreach (var value in values)
            {
                if (value.Type == ASDUType.M_SP_TB_1 ||
                    value.Type == ASDUType.M_DP_TB_1 ||
                    value.Type == ASDUType.M_ME_TF_1)
                {
                    if (!isInit)
                    {
                        currentType = value.Type;
                        isInit = true;
                        if (value.Type == ASDUType.M_SP_TB_1)
                        {
                            currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                            M_SP_TB_1_SingleArray[count++] = new M_SP_TB_1_Single(value.Address,
                                ((DiscretValue)value.Value).Value ? SIQ_Value.On : SIQ_Value.Off, 0,
                                ((DiscretValue)value.Value).ValueDt!.Value, 0);
                        }
                        else if (value.Type == ASDUType.M_DP_TB_1)
                        {
                            currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                            M_DP_TB_1_SingleArray[count++] = new M_DP_TB_1_Single(value.Address,
                                ((DiscretValue)value.Value).Value ? DIQ_Value.On : DIQ_Value.Off, 0,
                                ((DiscretValue)value.Value).ValueDt!.Value, 0);
                        }
                        else if (value.Type == ASDUType.M_ME_TF_1)
                        {
                            currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;
                            M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single(value.Address,
                                ((AnalogValue)value.Value).Value, 0,
                                ((AnalogValue)value.Value).ValueDt!.Value, 0);
                        }
                    }
                    else
                    {
                        if (currentType != value.Type || count == currentTypeMaxCount)
                        {
                            var headerReq = new ASDUPacketHeader_2_2(currentType, SQ.Single, count, cot, initAddr: initAddr,
                                            commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            if (currentType == ASDUType.M_SP_TB_1)
                            {
                                length = M_SP_TB_1_Single.Serialize(buffer, in headerReq, M_SP_TB_1_SingleArray, count);
                            }
                            else if (currentType == ASDUType.M_DP_TB_1)
                            {
                                length = M_DP_TB_1_Single.Serialize(buffer, in headerReq, M_DP_TB_1_SingleArray, count);
                            }
                            else if (currentType == ASDUType.M_ME_TF_1)
                            {
                                length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1_SingleArray, count);
                            }

                            _packetSender!.Send(buffer[..length], cot == COT.SPORADIC ? ChannelLayerPacketPriority.Low : ChannelLayerPacketPriority.Normal);

                            if (value.Type == ASDUType.M_SP_TB_1)
                                currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                            if (value.Type == ASDUType.M_DP_TB_1)
                                currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                            else if (value.Type == ASDUType.M_ME_TF_1)
                                currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;

                            currentType = value.Type;
                            count = 0;
                        }

                        if (value.Type == ASDUType.M_SP_TB_1)
                        {
                            M_SP_TB_1_SingleArray[count++] = new M_SP_TB_1_Single(value.Address,
                                ((DiscretValue)value.Value).Value ? SIQ_Value.On : SIQ_Value.Off, 0,
                                ((DiscretValue)value.Value).ValueDt!.Value, 0);
                        }
                        else if (value.Type == ASDUType.M_DP_TB_1)
                        {
                            M_DP_TB_1_SingleArray[count++] = new M_DP_TB_1_Single(value.Address,
                                ((DiscretValue)value.Value).Value ? DIQ_Value.On : DIQ_Value.Off, 0,
                                ((DiscretValue)value.Value).ValueDt!.Value, 0);
                        }
                        else if (value.Type == ASDUType.M_ME_TF_1)
                        {
                            M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single(value.Address,
                                ((AnalogValue)value.Value).Value, 0,
                                ((AnalogValue)value.Value).ValueDt!.Value, 0);
                        }
                    }
                }
            }

            if (count > 0)
            {
                var headerReq = new ASDUPacketHeader_2_2(currentType, SQ.Single, count, cot, initAddr: initAddr,
                                            commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                if (currentType == ASDUType.M_SP_TB_1)
                {
                    length = M_SP_TB_1_Single.Serialize(buffer, in headerReq, M_SP_TB_1_SingleArray, count);
                    currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                }
                else if (currentType == ASDUType.M_DP_TB_1)
                {
                    length = M_DP_TB_1_Single.Serialize(buffer, in headerReq, M_DP_TB_1_SingleArray, count);
                    currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                }
                else if (currentType == ASDUType.M_ME_TF_1)
                {
                    length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1_SingleArray, count);
                    currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;
                }

                _packetSender!.Send(buffer[..length], cot == COT.SPORADIC ? ChannelLayerPacketPriority.Low : ChannelLayerPacketPriority.Normal);
            }
        }
        finally
        {
            ArrayPool<M_SP_TB_1_Single>.Shared.Return(M_SP_TB_1_SingleArray);
            ArrayPool<M_DP_TB_1_Single>.Shared.Return(M_DP_TB_1_SingleArray);
            ArrayPool<M_ME_TF_1_Single>.Shared.Return(M_ME_TF_1_SingleArray);
        }
    }

    public IEC60870_5_104ServerApplicationLayer(IServiceProvider serviceProvider, IEC104ApplicationLayerModel applicationLayerOption,
        IDataSource<MapValueItem> dataSource,
        IDataProvider dataProvider,
        IChannelLayerPacketSender packetSender,
        IPhysicalLayerCommander physicalLayerCommander,
        ILogger<IEC60870_5_104ServerApplicationLayer> logger)
    {
        _timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        _readTransactionManager = new ApplicationLayerReadTransactionManager();
        _applicationLayerOption = applicationLayerOption;

        _dataProvider = dataProvider;
        _packetSender = packetSender;
        _physicalLayerCommander = physicalLayerCommander;
        _cts = new CancellationTokenSource();

        _logger = logger;

        _ = SendInRentBuffer(buffer =>
        {
            var headerReq = new ASDUPacketHeader_2_2(ASDUType.M_EI_NA_1, SQ.Single, 1,
            COT.INIT_MESSAGE,
            initAddr: 0, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
            var M_EI_NA_1 = new M_EI_NA_1(COI.Empty);
            var length = M_EI_NA_1.Serialize(buffer, in headerReq, in M_EI_NA_1);
            _packetSender!.Send(buffer[..length]);

            if (applicationLayerOption.SporadicSendEnabled)
            {
                _subscriber2 = new BatchSubscriber<MapValueItem>(_bufferizationSize, _bufferizationTimeout, dataSource, values =>
                {
                    if (values.TryGetNonEnumeratedCount(out var count) && count > 0)
                    {
                        return SendInRentBuffer(buffer =>
                        {
                            Stream(buffer, values);
                            return Task.CompletedTask;
                        });
                    }

                    return Task.CompletedTask;
                });
            }

            return Task.CompletedTask;
        });
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
        Process_C_IC_NA_1(header, address, qoi, _cts.Token);
    }

    void IASDUNotification.Notify_C_RD_NA(in ASDUPacketHeader_2_2 header, ushort address)
    {
        _logger.LogTrace("{@address}", address);
        Process_C_RD_NA_1(header, address, _cts.Token);
    }

    void IASDUNotification.Notify_C_CS_NA(in ASDUPacketHeader_2_2 header, ushort address, DateTime dateTime, TimeStatus timeStatus)
    {
        _logger.LogTrace("{@address} {@dateTime} {@timeStatus}", address, dateTime, timeStatus);
        Process_C_CS_NA_1(header, address, dateTime, timeStatus, _cts.Token);
    }

    void IASDUNotification.Notify_C_CI_NA(in ASDUPacketHeader_2_2 header, ushort address, QCC qcc)
    {
        _logger.LogTrace("{@address} {@qcc}", address, qcc);
        Process_C_CI_NA_1(header, address, qcc, _cts.Token);
    }

    void IASDUNotification.Notify_F_FR_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, uint lof, FRQ frq)
    {
        _logger.LogTrace("{@address} {@nof} {@lof} {@frq}", address, nof, lof, frq);
        Process_F_FR_NA_1(header, address, nof, lof, frq, _cts.Token);
    }

    void IASDUNotification.Notify_F_SR_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, uint los, SRQ frq)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@los} {@frq}", address, nof, nos, los, frq);
        Process_F_SR_NA_1(header, address, nof, nos, los, frq, _cts.Token);
    }

    void IASDUNotification.Notify_F_SC_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, SCQ scq)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@scq}", address, nof, nos, scq);
        Process_F_SC_NA_1(header, address, nof, nos, scq, _cts.Token);
    }

    void IASDUNotification.Notify_F_LS_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, LSQ lsq, byte chs)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@lsq} {@chs}", address, nof, nos, lsq, chs);
        Process_F_LS_NA_1(header, address, nof, nos, lsq, chs, _cts.Token);
    }

    void IASDUNotification.Notify_F_AF_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, AFQ afq)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@afq}", address, nof, nos, afq);
        Process_F_AF_NA_1(header, address, nof, nos, afq, _cts.Token);
    }

    void IASDUNotification.Notify_F_SG_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, Span<byte> segment)
    {
        _logger.LogTrace("{@address} {@nof} {@nos} {@segment.ToHex()}", address, nof, nos, segment.ToHex());
        Process_F_SG_NA_1(header, address, nof, nos, segment, _cts.Token);
    }

    void IASDUNotification.Notify_F_DR_TA(in ASDUPacketHeader_2_2 header, ushort address, ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus)
    {
        _logger.LogTrace("{@address} {@nodf} {@lof} {@sof} {@dateTime} {@timeStatus}", address, nodf, lof, sof, dateTime, timeStatus);
        Process_F_DR_TA_1(header, address, nodf, lof, sof, dateTime, timeStatus, _cts.Token);
    }

    void IASDUNotification.Notify_C_TS_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort fbp)
    {
        _logger.LogTrace("{@address} {@fbp}", address, fbp);
        Process_C_TS_NA_1(header, address, fbp, _cts.Token);
    }

    void IASDUNotification.Notify_C_TS_TA(in ASDUPacketHeader_2_2 header, ushort address, ushort tsc, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace("{@address} {@tsc} {@dateTime} {@status}", address, tsc, dateTime, status);
        Process_C_TS_TA_1(header, address, tsc, dateTime, status, _cts.Token);
    }

    void IASDUNotification.Notify_Unknown_Asdu_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        Process_Notify_Unknown_Asdu_Raw(header, asduInfoRaw, _cts.Token);
    }

    void IASDUNotification.Notify_Unknown_Cot_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        Process_Notify_Unknown_Cot_Raw(header, asduInfoRaw, _cts.Token);
    }

    void IASDUNotification.Notify_Unknown_Exception(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, Exception ex)
    {
        _logger.LogCritical("{@header} {@asduInfoRaw} {@ex}", header, asduInfoRaw.ToHex(), ex.GetInnerExceptionsString());
        Process_Notify_Unknown_Exception(header, asduInfoRaw, _cts.Token);
    }

    bool IASDUNotification.Notify_CommonAsduAddress(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        return Process_Notify_CommonAsduAddress(header, asduInfoRaw, _cts.Token);
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

