using Microsoft.Extensions.Logging;

using PowerUnit.Common.TimeoutService;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Application;
using PowerUnit.Service.IEC104.Channel.Events;
using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104.Channel;

/// <summary>
/// Канальный уровень
/// </summary>
public sealed class IEC60870_5_104ClientChannelLayer : IEC60870_5_104ChannelLayer
{
    private readonly string _id;

    public IEC60870_5_104ClientChannelLayer(
        string id,
        IPhysicalLayerCommander physicalLayerCommander,
        IECParserGenerator parserGenerator,
        TimeProvider timeProvider,
        ITimeoutService timeoutService,
        IIEC60870_5_104ChannelLayerDiagnostic diagnostic,
        ILogger<IEC60870_5_104ClientChannelLayer> logger) :
        base(physicalLayerCommander, parserGenerator, timeProvider, timeoutService, diagnostic, logger)
    {
        _id = id;
        // дружим каналку и сборщик пакета
        FrameDetector.AssignNotified(this);
    }

    protected override string Id => _id;

    private readonly IEC104ChannelLayerModel _iec104ChannelLayerModel = new IEC104ChannelLayerModel() { WindowWSize = 800, WindowKSize = 1200 };

    protected override IEC104ChannelLayerModel ChannelLayerModel => _iec104ChannelLayerModel;

    protected override bool IsServerSide => false;

    /// <summary>
    /// Обработка кадра без счетчиков
    /// </summary>
    /// <param name="control"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected sealed override async Task ProcessUPacket(UControl control, CancellationToken ct)
    {
        Logger.LogProcessUPacket(control);

        switch (control)
        {
            case UControl.StartDtCon:
                if (!_isEstablishedConnection)
                {
                    _asduNotification = new IEC60870_5_104ClientApplicationLayer();
                    _timeout0Id = await StopTimerAsync(_timeout0Id, ct);
                    _isEstablishedConnection = true;
                }

                break;
            case UControl.StopDtCon:
                if (_isEstablishedConnection)
                    _isEstablishedConnection = false;
                break;
            case UControl.TestFrCon:
                // остановить таймер 1
                _timeout1Id = await StopTimerAsync(_timeout1Id, ct);
                break;
            default:
                Diagnostic.ProtocolError(Id);
                PhysicalLayerController.DisconnectLayer();
                Logger.LogTrace("U packet not supported");
                break;
        }
    }

    protected sealed override async Task WorkCycleAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await foreach (var @event in Events.Reader.ReadAllAsync(ct))
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    if (@event is TimerEvent timerEvent)
                        Logger.LogTimerEvent(@event, timerEvent.TimerId);
                    else
                        Logger.LogEvent(@event);
                }

                switch (@event)
                {
                    case Channel.Events.ConnectEvent:
                        PhysicalLayerController.SendPacket(_startAct, 0, APCI.Size);
                        // взводим таймер ожидания идентификации соединения - должен придти правильный пакет от клиента
                        _timeout0Id = await StartTimerAsync(_timeout0Id, TimeSpan.FromSeconds(ChannelLayerModel.Timeout0Sec), ct);
                        break;
                    case Channel.Events.DisconnectEvent:
                        _isEstablishedConnection = false;
                        _asduNotification?.Dispose();
                        ResetCounter();
                        ResetTxQueue();
                        break;
                    case Channel.Events.TxEvent:
                        await ProcessTxQueue(ct);
                        await ProcessRxQueue(ct);
                        break;
                    case Channel.Events.RxEvent:
                        // перезапуск таймера 3 - только для клиента
                        _timeout3Id = await StartTimerAsync(_timeout3Id, TimeSpan.FromSeconds(ChannelLayerModel.Timeout3Sec), ct);
                        await ProcessRxQueue(ct);
                        await ProcessTxQueue(ct);
                        break;
                    case TimerEvent timer:
                        if (timer.TimerId == _timeout0Id)
                        {
                            await ProcessTimer0Async(ct);
                        }
                        else if (timer.TimerId == _timeout1Id)
                        {
                            await ProcessTimer1Async(ct);
                        }
                        else if (timer.TimerId == _timeout2Id)
                        {
                            await ProcessTimer2Async(ct);
                        }
                        else if (timer.TimerId == _timeout3Id)
                        {
                            await ProcessTimer3Async(ct);
                        }

                        break;
                    default:
                        break;
                }
            }
        }
    }
}

