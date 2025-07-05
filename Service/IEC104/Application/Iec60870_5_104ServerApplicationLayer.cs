using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Asdu;

using System.Buffers;

namespace PowerUnit;

public sealed partial class Iec60870_5_104ServerApplicationLayer : IAsduNotification
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;

    private readonly ApplicationLayerReadTransactionManager _readTransactionManager;

    private readonly IEC104ApplicationLayerModel _applicationLayerOption;

    private readonly IPhysicalLayerCommander _physicalLayerCommander;

    private readonly ILogger<Iec60870_5_104ServerApplicationLayer> _logger;

    private readonly IChannelLayerPacketSender _packetSender;

    private readonly CancellationTokenSource _cts;

    internal async Task SendInRentBuffer(Func<byte[], Task> action)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(256);
        try
        {
            await action(buffer);
        }
        catch (Iec60870_5_104ApplicationException iec104ApplicationException)
        {
            _logger.LogError(iec104ApplicationException, "Iec60870_5_104ServerApplicationLayer");

            var header = iec104ApplicationException.Header;
            var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
            COT.UNKNOWN_TRANSFER_REASON,
            pn: PN.Negative, tn: header.TN, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
            headerReq.SerializeUnsafe(buffer, 0);
            _packetSender!.Send(buffer[..AsduPacketHeader_2_2.Size]);
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

    public Iec60870_5_104ServerApplicationLayer(IServiceProvider serviceProvider, IEC104ApplicationLayerModel applicationLayerOption, IChannelLayerPacketSender packetSender, IPhysicalLayerCommander physicalLayerCommander, ILogger<Iec60870_5_104ServerApplicationLayer> logger)
    {
        _serviceProvider = serviceProvider;
        _timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        _readTransactionManager = new ApplicationLayerReadTransactionManager();
        _applicationLayerOption = applicationLayerOption;
        _packetSender = packetSender;
        _physicalLayerCommander = physicalLayerCommander;
        _cts = new CancellationTokenSource();
        _logger = logger;

        _ = SendInRentBuffer(buffer =>
        {
            var headerReq = new AsduPacketHeader_2_2(AsduType.M_EI_NA_1, SQ.Single, 1,
            COT.INIT_MESSAGE,
            initAddr: 0, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
            var M_EI_NA_1 = new M_EI_NA_1(COI.Empty);
            var length = M_EI_NA_1.Serialize(buffer, in headerReq, in M_EI_NA_1);
            _packetSender!.Send(buffer[..length]);
            return Task.CompletedTask;
        });
    }

    void IAsduNotification.Notify_M_SP(in AsduPacketHeader_2_2 header, ushort address, SIQ_Value value, SIQ_Status siq, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace($"{address} {value} {siq} {dateTime} {status}");
    }

    void IAsduNotification.Notify_M_DP(in AsduPacketHeader_2_2 header, ushort address, DIQ_Value value, DIQ_Status diq, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace($"{address} {value} {diq} {dateTime} {status}");
    }

    void IAsduNotification.Notify_M_ME(in AsduPacketHeader_2_2 header, ushort address, float value, QDS_Status qds, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace($"{address} {value} {qds} {dateTime} {status}");
    }

    void IAsduNotification.Notify_C_IC_NA(in AsduPacketHeader_2_2 header, ushort address, QOI qoi)
    {
        _logger.LogTrace($"{address} {qoi}");
        Process_C_IC_NA_1(header, address, qoi, _cts.Token);
    }

    void IAsduNotification.Notify_C_RD_NA(in AsduPacketHeader_2_2 header, ushort address)
    {
        _logger.LogTrace($"{address}");
        Process_C_RD_NA_1(header, address, _cts.Token);
    }

    void IAsduNotification.Notify_C_CS_NA(in AsduPacketHeader_2_2 header, ushort address, DateTime dateTime, TimeStatus timeStatus)
    {
        _logger.LogTrace($"{address} {dateTime} {timeStatus}");
        Process_C_CS_NA_1(header, address, dateTime, timeStatus, _cts.Token);
    }

    void IAsduNotification.Notify_C_CI_NA(in AsduPacketHeader_2_2 header, ushort address, QCC qcc)
    {
        _logger.LogTrace($"{address} {qcc}");
        Process_C_CI_NA_1(header, address, qcc, _cts.Token);
    }

    void IAsduNotification.Notify_F_FR_NA(in AsduPacketHeader_2_2 header, ushort address, ushort nof, uint lof, FRQ frq)
    {
        _logger.LogTrace($"{address} {nof} {lof} {frq}");
        Process_F_FR_NA_1(header, address, nof, lof, frq, _cts.Token);
    }

    void IAsduNotification.Notify_F_SR_NA(in AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, uint los, SRQ frq)
    {
        _logger.LogTrace($"{address} {nof} {nos} {los} {frq}");
        Process_F_SR_NA_1(header, address, nof, nos, los, frq, _cts.Token);
    }

    void IAsduNotification.Notify_F_SC_NA(in AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, SCQ scq)
    {
        _logger.LogTrace($"{address} {nof} {nos} {scq}");
        Process_F_SC_NA_1(header, address, nof, nos, scq, _cts.Token);
    }

    void IAsduNotification.Notify_F_LS_NA(in AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, LSQ lsq, byte chs)
    {
        _logger.LogTrace($"{address} {nof} {nos} {lsq} {chs}");
        Process_F_LS_NA_1(header, address, nof, nos, lsq, chs, _cts.Token);
    }

    void IAsduNotification.Notify_F_AF_NA(in AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, AFQ afq)
    {
        _logger.LogTrace($"{address} {nof} {nos} {afq}");
        Process_F_AF_NA_1(header, address, nof, nos, afq, _cts.Token);
    }

    void IAsduNotification.Notify_F_SG_NA(in AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, Span<byte> segment)
    {
        _logger.LogTrace($"{address} {nof} {nos} {segment.ToHex()}");
        Process_F_SG_NA_1(header, address, nof, nos, segment, _cts.Token);
    }

    void IAsduNotification.Notify_F_DR_TA(in AsduPacketHeader_2_2 header, ushort address, ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus)
    {
        _logger.LogTrace($"{address} {nodf} {lof} {sof} {dateTime} {timeStatus}");
        Process_F_DR_TA_1(header, address, nodf, lof, sof, dateTime, timeStatus, _cts.Token);
    }

    void IAsduNotification.Notify_C_TS_NA(in AsduPacketHeader_2_2 header, ushort address, ushort fbp)
    {
        _logger.LogTrace($"{address} {fbp}");
        Process_C_TS_NA_1(header, address, fbp, _cts.Token);
    }

    void IAsduNotification.Notify_C_TS_TA(in AsduPacketHeader_2_2 header, ushort address, ushort tsc, DateTime dateTime, TimeStatus status)
    {
        _logger.LogTrace($"{address} {tsc} {dateTime} {status}");
        Process_C_TS_TA_1(header, address, tsc, dateTime, status, _cts.Token);
    }

    void IAsduNotification.Notify_Unknown_Asdu_Raw(in AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        Process_Notify_Unknown_Asdu_Raw(header, asduInfoRaw, _cts.Token);
    }

    void IAsduNotification.Notify_Unknown_Cot_Raw(in AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        Process_Notify_Unknown_Cot_Raw(header, asduInfoRaw, _cts.Token);
    }

    void IAsduNotification.Notify_Unknown_Exception(in AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw, Exception ex)
    {
        _logger.LogCritical($"{header} {asduInfoRaw.ToHex()} {ex.GetInnerExceptionsString()}");
        Process_Notify_Unknown_Exception(header, asduInfoRaw, _cts.Token);
    }

    bool IAsduNotification.Notify_CommonAsduAddress(in AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        return Process_Notify_CommonAsduAddress(header, asduInfoRaw, _cts.Token);
    }

    void IDisposable.Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    void IAsduNotification.Notify_M_EI_NA(in AsduPacketHeader_2_2 header, ushort address, COI coi)
    {
    }
}

