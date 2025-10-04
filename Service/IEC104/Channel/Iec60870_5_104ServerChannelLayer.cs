using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Common.StructHelpers;
using PowerUnit.Common.Subsciption;
using PowerUnit.Common.TimeoutService;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Application;
using PowerUnit.Service.IEC104.Channel.Events;
using PowerUnit.Service.IEC104.Models;
using PowerUnit.Service.IEC104.Types;

using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Channel;

/// <summary>
/// Канальный уровень
/// </summary>
public sealed class IEC60870_5_104ServerChannelLayer : IEC60870_5_104ChannelLayer,
    IPhysicalLayerNotification,     // прием уведомлений от физического канала
    IChannelLayerPacketSender,      // прием запросов на передачу в канал
    IIEC60870_5_104NotifyPacket,    // прием уведомления о собранном кадре
    ITimeoutOwner             // прием уведомления о срабытывании таймера
{
    #region Счетчики пакетов

    /// <summary>
    /// Признак установленного соединения
    /// </summary>
    private bool _isEstablishedConnection;
    /// <summary>
    /// Счетчики принятых/переданных пакетов, обнуляются при переподключении
    /// </summary>
    private ushort _rxCounter, _txCounter;
    /// <summary>
    /// Счетчики несквитированных полученных/переданных пакетов
    /// </summary>
    private int _rxW, _txW;

    private void ResetCounter()
    {
        _rxCounter = _txCounter = 0;
        _rxW = _txW = 0;
    }

    #endregion

    #region Timers callback

    /// <summary>
    /// Идентификатор таймера при установки соединения
    /// </summary>
    private long? _timeout0Id;

    /// <summary>
    /// Идентификатор таймера при посылке или тестировании APDU
    /// </summary>
    private long? _timeout1Id;

    /// <summary>
    /// Идентификатор таймера для подтверждения в случае отсутствия сообщения с данными
    /// </summary>
    private long? _timeout2Id;

    /// <summary>
    /// Идентификатор таймера для посылки блоков тестирования в случае долгого простоя
    /// </summary>
    private long? _timeout3Id;

    /// <summary>
    /// Обработка таймера отсутсвия установки соединения после подключения
    /// </summary>
    /// <param name="timerId"></param>
    /// <returns></returns>
    private async Task ProcessTimer0Async(CancellationToken ct)
    {
        _timeout0Id = await StopTimerAsync(_timeout0Id, ct);
        _logger.LogTrace("Disconnect by timer0");
        _diagnostic.ProtocolError(_serverModel.ServerName);
        _physicalLayerController.DisconnectLayer();
    }

    /// <summary>
    /// Обработка таймера 
    /// </summary>
    /// <returns></returns>
    private async Task ProcessTimer1Async(CancellationToken ct)
    {
        _timeout1Id = await StopTimerAsync(_timeout1Id, ct);
        _logger.LogTrace("Disconnect by timer1");
        _diagnostic.ProtocolError(_serverModel.ServerName);
        _physicalLayerController.DisconnectLayer();
    }

    /// <summary>
    /// Обработка таймера
    /// </summary>
    /// <returns></returns>
    private async Task ProcessTimer2Async(CancellationToken ct)
    {
        _timeout2Id = await StopTimerAsync(_timeout2Id, ct);
        // отправка квитирующего S пакета
        new APCI(PacketS.Size, new PacketS(_rxCounter)).SerializeUnsafe(_buffer, 0);
        _physicalLayerController.SendPacket(_buffer, 0, APCI.Size);
        _diagnostic.SendSPacket(_serverModel.ServerName);
        _logger.LogTimer2(_rxCounter);

        _rxW = 0;
    }

    /// <summary>
    /// Обработка таймера
    /// </summary>
    /// <returns></returns>
    private async Task ProcessTimer3Async(CancellationToken ct)
    {
        _timeout3Id = await StopTimerAsync(_timeout3Id, ct);

        // отправка U Test пакета
        _physicalLayerController.SendPacket(_testAct, 0, APCI.Size);
        _diagnostic.SendUPacket(_serverModel.ServerName);
        _logger.LogTimer3(UControl.TestFrAct);

        _timeout1Id = await StartTimerAsync(_timeout1Id, TimeSpan.FromSeconds(_serverModel.ChannelLayerModel.Timeout1Sec), ct);
        _timeout3Id = await StartTimerAsync(_timeout3Id, TimeSpan.FromSeconds(_serverModel.ChannelLayerModel.Timeout3Sec), ct);
    }

    private async Task ResetTimers(CancellationToken ct)
    {
        _timeout1Id = await StopTimerAsync(_timeout1Id, ct);
        _timeout2Id = await StopTimerAsync(_timeout2Id, ct);
        _timeout3Id = await StopTimerAsync(_timeout3Id, ct);
    }

    #endregion

    private readonly IServiceProvider _serviceProvider;

    private readonly TimeProvider _timeProvider;

    private IASDUNotification? _asduNotification;

    private readonly ITimeoutService _timeoutsService;

    private readonly IEC104ServerModel _serverModel;

    private readonly IDataSource<MapValueItem> _dataSource;
    private readonly IDataProvider _dataProvider;
    private readonly IIEC60870_5_104ChannelLayerDiagnostic _diagnostic;

    private readonly IPhysicalLayerCommander _physicalLayerController;

    private readonly IECParserGenerator _parserGenerator;

    private readonly CancellationToken _ct;

    private readonly ILogger<IEC60870_5_104ServerChannelLayer> _logger;

    /// <summary>
    /// Подтверждение подключения
    /// </summary>
    private static readonly byte[] _startCon = new byte[APCI.Size];
    /// <summary>
    /// Подтверждение отключения
    /// </summary>
    private static readonly byte[] _stopCon = new byte[APCI.Size];
    /// <summary>
    /// Активация тестовой посылки
    /// </summary>
    private static readonly byte[] _testAct = new byte[APCI.Size];
    /// <summary>
    /// Подтверждение тестовой посылки
    /// </summary>
    private static readonly byte[] _testCon = new byte[APCI.Size];

    /// <summary>
    /// Т.к. все события обрабатывает 1 цикл и за его пределами пересылок не выполняется
    /// то достаточто 1 буфера для передачи всех посылок
    /// </summary>
    private readonly byte[] _buffer = new byte[256];

    /// <summary>
    /// Статический конструктор. Можно было бы обойтись без него, но хочется что бы константные запросы заполнились
    /// из Packet структур, так удобнее
    /// </summary>
    static IEC60870_5_104ServerChannelLayer()
    {
        new APCI(PacketU.Size, new PacketU(UControl.StartDtCon)).SerializeUnsafe(_startCon, 0);
        new APCI(PacketU.Size, new PacketU(UControl.StopDtCon)).SerializeUnsafe(_stopCon, 0);
        new APCI(PacketU.Size, new PacketU(UControl.TestFrAct)).SerializeUnsafe(_testAct, 0);
        new APCI(PacketU.Size, new PacketU(UControl.TestFrCon)).SerializeUnsafe(_testCon, 0);
    }

    private static IEC60870_5_104ServerApplicationLayer GetApplicationLayer(IServiceProvider serviceProvider, IEC104ServerModel serverOptions,
        IDataSource<MapValueItem> dataSource,
        IDataProvider dataProvider,
        IChannelLayerPacketSender channelLayerPacketSender, IPhysicalLayerCommander physicalLayerCommander)
    {
        var serverApplicationLayer = ActivatorUtilities.CreateInstance<IEC60870_5_104ServerApplicationLayer>(serviceProvider, [serverOptions, dataSource, dataProvider,
            channelLayerPacketSender, physicalLayerCommander]);

        return serverApplicationLayer;
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
        ITimeoutService timeoutService,
        CancellationToken ct)
    {
        _serviceProvider = serviceProvider;
        _timeProvider = timeProvider;
        _timeoutsService = timeoutService;
        _physicalLayerController = physicalLayerController;
        _serverModel = serverModel;
        _dataSource = dataSource;
        _dataProvider = dataProvider;
        _diagnostic = diagnostic;
        _parserGenerator = parserGenerator;
        _ct = ct;
        _logger = logger;
        // дружим каналку и сборщик пакета
        FrameDetector.AssignNotified(this);
    }

    #region Обработка таймеров

    /// <summary>
    /// Запуск таймера или его создание при необходимости
    /// </summary>
    /// <param name="timerId"></param>
    /// <param name="timeout"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async ValueTask<long> StartTimerAsync(long? timerId, TimeSpan timeout, CancellationToken ct)
    {
        if (timerId.HasValue)
        {
            await _timeoutsService.RestartTimeoutAsync(this, timerId.Value, timeout, ct);
            return timerId.Value;
        }
        else
        {
            return await _timeoutsService.CreateTimeoutAsync(this, timeout, ct);
        }
    }

    /// <summary>
    /// Останов таймера
    /// </summary>
    /// <param name="timerId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async ValueTask<long?> StopTimerAsync(long? timerId, CancellationToken ct)
    {
        if (timerId.HasValue)
        {
            await _timeoutsService.CancelTimeoutAsync(this, timerId.Value, ct);
        }

        return null;
    }

    #endregion

    #region Обработка передаваемых пакетов

    /// <summary>
    /// Передача пользовательских пакетов
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task ProcessTxQueue(CancellationToken ct)
    {
        // только на установленном соединении
        if (!_isEstablishedConnection)
        {
            _logger.LogTrace("ProcessTxQueue connection not established");
            return;
        }

        // забираем в очередь передачи
        var txQueueCount = TxQueue.Reader.Count;
        while (txQueueCount > 0)
        {
            var sendMsg = await TxQueue.Reader.ReadAsync(ct);
            txQueueCount--;
            if (sendMsg.Priority == ChannelLayerPacketPriority.Low && TxButNotAckQueue.Count > _serverModel.ChannelLayerModel.MaxQueueSize)
            {
                _diagnostic.AppMsgSkip(_serverModel.ServerName, sendMsg.Priority);
                continue;
            }
            else
            {
                TxButNotAckQueue.AddLast(sendMsg);
                _diagnostic.AppMsgSend(_serverModel.ServerName, sendMsg.Priority);
            }
        }

        // только если позволяет окно
        if (_txW >= _serverModel.ChannelLayerModel.WindowKSize)
        {
            return;
        }

        var packetToSendCount = TxButNotAckQueue.Count;

        if (packetToSendCount > 0)
        {
            packetToSendCount = Math.Min(packetToSendCount, _serverModel.ChannelLayerModel.WindowKSize - _txW);

            var skip = _txW;
            var txPacket = TxButNotAckQueue.First;
            while (skip > 0)
            {
                txPacket = txPacket!.Next;
                skip--;
            }

            while (packetToSendCount > 0 && _txW < TxButNotAckQueue.Count)
            {
                var msg = txPacket!.ValueRef;
                new APCI((byte)(PacketI.Size + msg.MsgLength), new PacketI(_txCounter, _rxCounter)).SerializeUnsafe(_buffer, 0);

                // Хорошая задумка, но похоже некоторые клиенты могут ожидать ответ только в одном пакете не получив его, они вполне могут разорвать связь поэтому вынесем в тонкую настройку
                if (_serverModel.ChannelLayerModel.UseFragmentSend)
                {
                    _physicalLayerController.SendPacket(_buffer, 0, APCI.Size);
                    _physicalLayerController.SendPacket(msg.Msg, 0, msg.MsgLength);
                }
                else
                {
                    msg.Msg.AsSpan(0, msg.MsgLength).CopyTo(_buffer.AsSpan(APCI.Size));
                    _physicalLayerController.SendPacket(_buffer, 0, APCI.Size + msg.MsgLength);
                }

                _diagnostic.SendIPacket(_serverModel.ServerName);
                _logger.LogProcessTxQueue(_rxCounter, _txCounter);

                _rxW = 0; // сброс счетчика несквитированных посылок
                _txCounter = CounterIncrement(_txCounter); // наращиваем счетчик отправленных
                _txW++; // наращиваем счетчик не квитированных
                packetToSendCount--;
                txPacket = txPacket.Next;
            }

            // остановить таймер 2
            _timeout2Id = await StopTimerAsync(_timeout2Id, ct);
            // перезапуск таймера 1
            _timeout1Id = await StartTimerAsync(_timeout1Id, TimeSpan.FromSeconds(_serverModel.ChannelLayerModel.Timeout1Sec), ct);
        }
    }

    #endregion

    #region Обработка принимаемых пакетов

    /// <summary>
    /// Обработка сквитированных пришедшим пакетом, пакетов в очереди на отправку
    /// </summary>
    /// <param name="dountAckPacket"></param>
    private async Task ProcessRecievedAck(int dountAckPacket, CancellationToken ct)
    {
        // остановить таймер 1
        _timeout1Id = await StopTimerAsync(_timeout1Id, ct);

        var ackCount = _txW - dountAckPacket;
        _txW -= ackCount; // уменьшаем на количество подтвержденных пакетов

        _logger.LogProcessRecievedAck(dountAckPacket, ackCount);

        while (ackCount > 0)
        {
            TxButNotAckQueue.First().Dispose();
            TxButNotAckQueue.RemoveFirst();
            ackCount--;
        }
    }

    /// <summary>
    /// Обработка информационного кадра
    /// </summary>
    /// <param name="rx"></param>
    /// <param name="tx"></param>
    /// <param name="data"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task ProcessIPacket(ushort rx, ushort tx, byte[] data, CancellationToken ct)
    {
        _logger.LogProcessIPacket(rx, tx);

        var txDelta = CalcUnAckPacket(_txCounter, rx);
        var counterOk = txDelta >= 0 && txDelta <= _txW && tx == _rxCounter;

        if (counterOk)
        {
            _rxCounter = CounterIncrement(_rxCounter);    // Принятый пакет
            _rxW++;  // не сквитированный пакет

            await ProcessRecievedAck(txDelta, ct);

            if (_rxW >= _serverModel.ChannelLayerModel.WindowWSize)
            {
                new APCI(PacketS.Size, new PacketS(_rxCounter)).SerializeUnsafe(_buffer, 0);
                _physicalLayerController.SendPacket(_buffer, 0, APCI.Size);
                _diagnostic.SendSPacket(_serverModel.ServerName);
                _rxW = 0; // несквитированные мною

                _logger.LogProcessIPacket2(_rxCounter);
                // остановить таймер 2
                _timeout2Id = await StopTimerAsync(_timeout2Id, ct);
            }
            else
            {
                // перезапуск таймера 2
                _timeout2Id = await StartTimerAsync(_timeout2Id, TimeSpan.FromSeconds(_serverModel.ChannelLayerModel.Timeout2Sec), ct);
            }

            if (_asduNotification != null)
                _parserGenerator.Parse(_asduNotification, data, _timeProvider.GetUtcNow().DateTime, true);
        }
        else
        {
            // нарушение протокола - разрыв соединения по нашей инициативе
            _logger.LogTrace("Disconnect by IPacket Rcv corruption");
            _diagnostic.ProtocolError(_serverModel.ServerName);
            _physicalLayerController.DisconnectLayer();
        }
    }

    /// <summary>
    /// Обработка кадра подтверждения
    /// </summary>
    /// <param name="rx"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task ProcessSPacket(ushort rx, CancellationToken ct)
    {
        _logger.LogProcessSPacket(rx);

        var txDelta = CalcUnAckPacket(_txCounter, rx);
        var counterOk = txDelta >= 0 && txDelta <= _txW;

        if (counterOk)
        {
            await ProcessRecievedAck(txDelta, ct);
        }
        else
        {
            // нарушение протокола - разрыв соединения по нашей инициативе
            _logger.LogTrace("Disconnect by SPacket Rcv corruption");
            _diagnostic.ProtocolError(_serverModel.ServerName);
            _physicalLayerController.DisconnectLayer();
        }
    }

    /// <summary>
    /// Обработка кадра без счетчиков
    /// </summary>
    /// <param name="control"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task ProcessUPacket(UControl control, CancellationToken ct)
    {
        _logger.LogProcessUPacket(control);

        switch (control)
        {
            case UControl.StartDtAct:
                if (!_isEstablishedConnection)
                {
                    _asduNotification = GetApplicationLayer(_serviceProvider, _serverModel, _dataSource, _dataProvider, this, _physicalLayerController);

                    _isEstablishedConnection = true;
                    _timeout0Id = await StopTimerAsync(_timeout0Id, ct);
                }

                _physicalLayerController.SendPacket(_startCon, 0, APCI.Size);
                _diagnostic.SendUPacket(_serverModel.ServerName);
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

                _physicalLayerController.SendPacket(_stopCon, 0, APCI.Size);
                _diagnostic.SendUPacket(_serverModel.ServerName);
                break;
            case UControl.TestFrAct:
                if (_isEstablishedConnection)
                {
                    _physicalLayerController.SendPacket(_testCon, 0, APCI.Size);
                }

                break;
            //case UControl.StartDtCon:
            //    break;
            //case UControl.StopDtCon:
            //    break;
            case UControl.TestFrCon:
                // остановить таймер 1
                _timeout1Id = await StopTimerAsync(_timeout1Id, ct);
                break;
            default:
                _diagnostic.ProtocolError(_serverModel.ServerName);
                _physicalLayerController.DisconnectLayer();
                _logger.LogTrace("U packet undefuned");
                break;
        }
    }

    /// <summary>
    /// Обработка очередного принятого кадра
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task ProcessRxQueue(CancellationToken ct)
    {
        var cnt = RxQueue.Reader.Count;

        while (cnt != 0)
        {
            cnt--;

            var rxPacket = await RxQueue.Reader.ReadAsync(ct);

            var apci_2 = MemoryMarshal.AsRef<APCI>(rxPacket.AsSpan(0, APCI.Size));

            if (apci_2.TryGetIPacket(out var iPacket))
            {
                _diagnostic.RcvIPacket(_serverModel.ServerName);
                await ProcessIPacket(iPacket.Rx, iPacket.Tx, rxPacket[6..], ct);
            }
            else if (apci_2.TryGetUPacket(out var uPacket))
            {
                _diagnostic.RcvUPacket(_serverModel.ServerName);
                await ProcessUPacket(uPacket.UControl, ct);
            }
            else if (apci_2.TryGetSPacket(out var sPacket))
            {
                _diagnostic.RcvSPacket(_serverModel.ServerName);
                await ProcessSPacket(sPacket.Rx, ct);
            }
            else
            {
                _logger.LogTrace("Undefined packet rcv");
                _diagnostic.ProtocolError(_serverModel.ServerName);
                _physicalLayerController.DisconnectLayer();
                return;
            }
        }
    }

    #endregion

    /// <summary>
    /// Исполнение внешних воздействий
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task WorkCycleAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await foreach (var @event in Events.Reader.ReadAllAsync(ct))
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    if (@event is TimerEvent timerEvent)
                        _logger.LogTimerEvent(@event, timerEvent.TimerId);
                    else
                        _logger.LogEvent(@event);
                }

                switch (@event)
                {
                    case Channel.Events.ConnectEvent:
                        // взводим таймер ожидания идентификации соединения - должен придти правильный пакет от клиента
                        _timeout0Id = await StartTimerAsync(_timeout0Id, TimeSpan.FromSeconds(_serverModel.ChannelLayerModel.Timeout0Sec), ct);
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
                        // перезапуск таймера 3
                        _timeout3Id = await StartTimerAsync(_timeout3Id, TimeSpan.FromSeconds(_serverModel.ChannelLayerModel.Timeout3Sec), ct);
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

    /// <summary>
    /// Подключение физического уровня
    /// </summary>
    void IPhysicalLayerNotification.Connect()
    {
        Events.Writer.TryWrite(ConnectEvent);
        // запускаем рабочий поток
        WorkCycle ??= Task.Run(() => WorkCycleAsync(_ct));
    }

    /// <summary>
    /// Отключение физического уровня
    /// </summary>
    void IPhysicalLayerNotification.Disconnect() => Events.Writer.TryWrite(DisconnectEvent);

    /// <summary>
    /// Принятый пакет из физического уровня нужно продетектировать
    /// </summary>
    /// <param name="packet"></param>
    void IPhysicalLayerNotification.Recieve(byte[] packet) => FrameDetector.TryGetFrame(packet);

    /// <summary>
    /// Собран очередной пакет по протоколу
    /// </summary>
    /// <param name="packet"></param>
    void INotifyPacket.NotifyPacketDetected(byte[] packet)
    {
        RxQueue.Writer.TryWrite(packet);
        Events.Writer.TryWrite(RxEvent);
    }

    /// <summary>
    /// Помещение пакета в очередь на передачу
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    void IChannelLayerPacketSender.Send(ReadOnlySpan<byte> packet, ChannelLayerPacketPriority priority)
    {
        _diagnostic.AppMsgTotal(_serverModel.ServerName, priority);

        TxQueue.Writer.TryWrite(new SendMsg(packet, priority));
        Events.Writer.TryWrite(TxEvent);
    }

    void IChannelLayerPacketSender.Send(byte[] packet, ChannelLayerPacketPriority priority)
    {
        _diagnostic.AppMsgTotal(_serverModel.ServerName, priority);

        TxQueue.Writer.TryWrite(new SendMsg(packet, priority));
        Events.Writer.TryWrite(TxEvent);
    }

    void ITimeoutOwner.NotifyTimeoutReady(long timeout) => Events.Writer.TryWrite(new TimerEvent(timeout));
}

