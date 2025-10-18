using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Common.Subsciption;
using PowerUnit.Common.TimeoutService;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Application;
using PowerUnit.Service.IEC104.Channel.Events;
using PowerUnit.Service.IEC104.Models;
using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104.Channel;

/// <summary>
/// Канальный уровень
/// </summary>
public sealed class IEC60870_5_104ServerChannelLayer : IEC60870_5_104ChannelLayer
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IEC104ServerModel _serverModel;

    private readonly IDataSource<MapValueItem> _dataSource;
    private readonly IDataProvider _dataProvider;

    private static IEC60870_5_104ServerApplicationLayer GetApplicationLayer(IServiceProvider serviceProvider, IEC104ServerModel serverOptions,
        IDataSource<MapValueItem> dataSource,
        IDataProvider dataProvider,
        IChannelLayerPacketSender channelLayerPacketSender, IPhysicalLayerCommander physicalLayerCommander)
    {
        return ActivatorUtilities.CreateInstance<IEC60870_5_104ServerApplicationLayer>(serviceProvider,
            [serverOptions, dataSource, dataProvider, channelLayerPacketSender, physicalLayerCommander]);
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    public IEC60870_5_104ServerChannelLayer(IServiceProvider serviceProvider, IPhysicalLayerCommander physicalLayerController,
        IEC104ServerModel serverModel,
        IDataSource<MapValueItem> dataSource,
        IDataProvider dataProvider,
        IECParserGenerator parserGenerator,
        IIEC60870_5_104ChannelLayerDiagnostic diagnostic,
        ILogger<IEC60870_5_104ServerChannelLayer> logger,
        TimeProvider timeProvider,
        ITimeoutService timeoutService) : base(physicalLayerController, parserGenerator, timeProvider, timeoutService, diagnostic, logger)
    {
        _serviceProvider = serviceProvider;
        _serverModel = serverModel;
        _dataSource = dataSource;
        _dataProvider = dataProvider;
        // дружим каналку и сборщик пакета
        FrameDetector.AssignNotified(this);
    }

    protected sealed override string Id => _serverModel.ServerName;

    protected sealed override IEC104ChannelLayerModel ChannelLayerModel => _serverModel.ChannelLayerModel;
    protected sealed override bool IsServerSide => true;

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
            case UControl.StartDtAct:
                if (!_isEstablishedConnection)
                {
                    _asduNotification = GetApplicationLayer(_serviceProvider, _serverModel, _dataSource, _dataProvider, this, PhysicalLayerController);

                    _isEstablishedConnection = true;
                    _timeout0Id = await StopTimerAsync(_timeout0Id, ct);
                }

                PhysicalLayerController.SendPacket(_startCon, 0, APCI.Size);
                Diagnostic.SendUPacket(Id);
                break;
            case UControl.StopDtAct:
                // Подумать - клиент может отключиться, но при этом не разорвать соединение
                // в этом случае, наверное имеет смысл взвести таймер ожидания подключения еще раз
                // и разорвать соединение если он истечет, а клиент так и не подключится
                if (_isEstablishedConnection)
                {
                    _isEstablishedConnection = false;
                    _asduNotification?.Dispose();
                    ResetCounter();
                    ResetTxQueue();
                    // таймаут разрыва соединения без длительного подключения
                    _timeout0Id = await StartTimerAsync(_timeout0Id, TimeSpan.FromSeconds(_serverModel.ChannelLayerModel.Timeout0Sec), ct);
                    // остальные таймеры не нужны
                    await ResetTimers(ct);
                }

                PhysicalLayerController.SendPacket(_stopCon, 0, APCI.Size);
                Diagnostic.SendUPacket(Id);
                break;
            case UControl.TestFrAct:
                if (_isEstablishedConnection)
                {
                    PhysicalLayerController.SendPacket(_testCon, 0, APCI.Size);
                }

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
                        // взводим таймер ожидания идентификации соединения - должен придти правильный пакет от клиента
                        _timeout0Id = await StartTimerAsync(_timeout0Id, TimeSpan.FromSeconds(ChannelLayerModel.Timeout0Sec), ct);
                        break;
                    case Channel.Events.DisconnectEvent:
                        _asduNotification?.Dispose();
                        WorkCycle = null;
                        break;
                    case Channel.Events.TxEvent:
                        await ProcessTxQueue(ct);
                        await ProcessRxQueue(ct);
                        break;
                    case Channel.Events.RxEvent:
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

