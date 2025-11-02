using Microsoft.Extensions.Logging;

using PowerUnit.Common.StructHelpers;
using PowerUnit.Common.TimeoutService;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Channel.Events;
using PowerUnit.Service.IEC104.Types;

using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace PowerUnit.Service.IEC104.Channel;

public abstract class IEC60870_5_104ChannelLayer : IPhysicalLayerNotification,     // прием уведомлений от физического канала
    IChannelLayerPacketSender,      // прием запросов на передачу в канал
    IIEC60870_5_104NotifyPacket,    // прием уведомления о собранном кадре
    ITimeoutOwner,             // прием уведомления о срабытывании таймера
    IDisposable
{
    protected IPhysicalLayerCommander PhysicalLayerController { get; }
    protected IECParserGenerator ParserGenerator { get; }
    private readonly TimeProvider _timeProvider;
    private readonly ITimeoutService _timeoutsService;
    protected IIEC60870_5_104ChannelLayerDiagnostic Diagnostic { get; }
    protected ILogger Logger { get; }
    protected readonly CancellationTokenSource Cts = new CancellationTokenSource();

    #region Счетчики пакетов

    /// <summary>
    /// Признак установленного соединения
    /// </summary>
    protected bool _isEstablishedConnection;
    /// <summary>
    /// Счетчики принятых/переданных пакетов, обнуляются при переподключении
    /// </summary>
    protected ushort _rxCounter, _txCounter;
    /// <summary>
    /// Счетчики несквитированных полученных/переданных пакетов
    /// </summary>
    protected int _rxW, _txW;

    protected void ResetCounter()
    {
        _rxCounter = _txCounter = 0;
        _rxW = _txW = 0;
    }

    #endregion

    /// <summary>
    /// Т.к. все события обрабатывает 1 цикл и за его пределами пересылок не выполняется
    /// то достаточто 1 буфера для передачи всех посылок
    /// </summary>
    private readonly byte[] _buffer = new byte[256];

    /// <summary>
    /// Активизация подключения
    /// </summary>
    protected static readonly byte[] _startAct = new byte[APCI.Size];
    /// <summary>
    /// Подтверждение подключения
    /// </summary>
    protected static readonly byte[] _startCon = new byte[APCI.Size];
    /// <summary>
    /// Активизация отключения
    /// </summary>
    protected static readonly byte[] _stopAct = new byte[APCI.Size];
    /// <summary>
    /// Подтверждение отключения
    /// </summary>
    protected static readonly byte[] _stopCon = new byte[APCI.Size];
    /// <summary>
    /// Активация тестовой посылки
    /// </summary>
    protected static readonly byte[] _testAct = new byte[APCI.Size];
    /// <summary>
    /// Подтверждение тестовой посылки
    /// </summary>
    protected static readonly byte[] _testCon = new byte[APCI.Size];

    /// <summary>
    /// Статический конструктор. Можно было бы обойтись без него, но хочется что бы константные запросы заполнились
    /// из Packet структур, так удобнее
    /// </summary>
    static IEC60870_5_104ChannelLayer()
    {
        new APCI(PacketU.Size, new PacketU(UControl.StartDtAct)).SerializeUnsafe(_startAct, 0);
        new APCI(PacketU.Size, new PacketU(UControl.StartDtCon)).SerializeUnsafe(_startCon, 0);
        new APCI(PacketU.Size, new PacketU(UControl.StopDtAct)).SerializeUnsafe(_stopCon, 0);
        new APCI(PacketU.Size, new PacketU(UControl.StopDtCon)).SerializeUnsafe(_stopCon, 0);
        new APCI(PacketU.Size, new PacketU(UControl.TestFrAct)).SerializeUnsafe(_testAct, 0);
        new APCI(PacketU.Size, new PacketU(UControl.TestFrCon)).SerializeUnsafe(_testCon, 0);
    }

    public IEC60870_5_104ChannelLayer(IPhysicalLayerCommander physicalLayerCommander, IECParserGenerator parserGenerator,
        TimeProvider timeProvider,
        ITimeoutService timeoutService,
        IIEC60870_5_104ChannelLayerDiagnostic diagnostic,
        ILogger logger)
    {
        PhysicalLayerController = physicalLayerCommander;
        ParserGenerator = parserGenerator;
        _timeProvider = timeProvider;
        _timeoutsService = timeoutService;
        Diagnostic = diagnostic;
        Logger = logger;
    }

    /// <summary>
    /// Сборщик пакета
    /// </summary>
    protected IFrameDetector FrameDetector { get; } = new IEC60870_5_104FrameDetector();
    /// <summary>
    /// Очередь происходящих событий
    /// </summary>
    protected Channel<IEvent> Events { get; } = System.Threading.Channels.Channel.CreateUnbounded<IEvent>();
    /// <summary>
    /// Событие подключения
    /// </summary>
    protected static IEvent ConnectEvent { get; } = new ConnectEvent();
    /// <summary>
    /// Событие отключения
    /// </summary>
    protected static IEvent DisconnectEvent { get; } = new DisconnectEvent();
    /// <summary>
    /// Принят пакет
    /// </summary>
    protected static IEvent RxEvent { get; } = new RxEvent();
    /// <summary>
    /// Появился пакет на передачу
    /// </summary>
    protected static IEvent TxEvent { get; } = new TxEvent();

    /// <summary>
    /// Очередь принятых пакетов
    /// </summary>
    protected Channel<byte[]> RxQueue { get; } = System.Threading.Channels.Channel.CreateUnbounded<byte[]>();

    /// <summary>
    /// Очередь пакетов на передачу
    /// </summary>
    protected Channel<SendMsg> TxQueue { get; } =
        System.Threading.Channels.Channel.CreateUnbounded<SendMsg>();

    /// <summary>
    /// Очередь переданных, но не подтвержденных пакетов
    /// </summary>
    protected LinkedList<SendMsg> TxButNotAckQueue { get; } = [];

    protected IASDUNotification? _asduNotification;

    /// <summary>
    /// Рабочий цикл
    /// </summary>
    protected Task? WorkCycle { get; set; }

    protected void ResetTxQueue()
    {
        TxButNotAckQueue.Clear();
    }

    /// <summary>
    /// Инкремент с переполнением
    /// </summary>
    /// <param name="counter"></param>
    /// <returns></returns>
    protected static ushort CounterIncrement(ushort counter)
    {
        counter++;
        counter &= 0x7FFF;
        return counter;
    }

    /// <summary>
    /// Расчет количества переданных, но не сквитированных другой стороной пакетов
    /// </summary>
    /// <param name="packetRx">счетчик принятых другой стороной пакетов</param>
    /// <returns></returns>
    protected static int CalcUnAckPacket(ushort txCounter, ushort packetRx)
    {
        return txCounter >= packetRx ? txCounter - packetRx : (ushort.MaxValue >> 1) - packetRx + txCounter + 1;
    }

    #region Timers callback

    /// <summary>
    /// Идентификатор таймера при установки соединения
    /// </summary>
    protected long? _timeout0Id;

    /// <summary>
    /// Идентификатор таймера при посылке или тестировании APDU
    /// </summary>
    protected long? _timeout1Id;

    /// <summary>
    /// Идентификатор таймера для подтверждения в случае отсутствия сообщения с данными
    /// </summary>
    protected long? _timeout2Id;

    /// <summary>
    /// Идентификатор таймера для посылки блоков тестирования в случае долгого простоя
    /// </summary>
    protected long? _timeout3Id;

    /// <summary>
    /// Обработка таймера отсутсвия установки соединения после подключения
    /// </summary>
    /// <param name="timerId"></param>
    /// <returns></returns>
    protected async Task ProcessTimer0Async(CancellationToken ct)
    {
        _timeout0Id = await StopTimerAsync(_timeout0Id, ct);
        Logger.LogTrace("Disconnect by timer0");
        Diagnostic.ProtocolError(Id);
        PhysicalLayerController.DisconnectLayer();
    }

    /// <summary>
    /// Обработка таймера 
    /// </summary>
    /// <returns></returns>
    protected async Task ProcessTimer1Async(CancellationToken ct)
    {
        _timeout1Id = await StopTimerAsync(_timeout1Id, ct);
        Logger.LogTrace("Disconnect by timer1");
        Diagnostic.ProtocolError(Id);
        PhysicalLayerController.DisconnectLayer();
    }

    /// <summary>
    /// Обработка таймера
    /// </summary>
    /// <returns></returns>
    protected async Task ProcessTimer2Async(CancellationToken ct)
    {
        _timeout2Id = await StopTimerAsync(_timeout2Id, ct);
        // отправка квитирующего S пакета
        new APCI(PacketS.Size, new PacketS(_rxCounter)).SerializeUnsafe(_buffer, 0);
        PhysicalLayerController.SendPacket(_buffer, 0, APCI.Size);
        Diagnostic.SendSPacket(Id);
        Logger.LogTimer2(_rxCounter);

        _rxW = 0;
    }

    /// <summary>
    /// Обработка таймера
    /// </summary>
    /// <returns></returns>
    protected async Task ProcessTimer3Async(CancellationToken ct)
    {
        _timeout3Id = await StopTimerAsync(_timeout3Id, ct);

        // отправка U Test пакета
        PhysicalLayerController.SendPacket(_testAct, 0, APCI.Size);
        Diagnostic.SendUPacket(Id);
        Logger.LogTimer3(UControl.TestFrAct);

        _timeout1Id = await StartTimerAsync(_timeout1Id, TimeSpan.FromSeconds(ChannelLayerModel.Timeout1Sec), ct);
        _timeout3Id = await StartTimerAsync(_timeout3Id, TimeSpan.FromSeconds(ChannelLayerModel.Timeout3Sec), ct);
    }

    protected async Task ResetTimers(CancellationToken ct)
    {
        _timeout1Id = await StopTimerAsync(_timeout1Id, ct);
        _timeout2Id = await StopTimerAsync(_timeout2Id, ct);
        _timeout3Id = await StopTimerAsync(_timeout3Id, ct);
    }

    #endregion

    #region Обработка таймеров

    /// <summary>
    /// Запуск таймера или его создание при необходимости
    /// </summary>
    /// <param name="timerId"></param>
    /// <param name="timeout"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected async ValueTask<long> StartTimerAsync(long? timerId, TimeSpan timeout, CancellationToken ct)
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
    protected async ValueTask<long?> StopTimerAsync(long? timerId, CancellationToken ct)
    {
        if (timerId.HasValue)
        {
            await _timeoutsService.CancelTimeoutAsync(this, timerId.Value, ct);
        }

        return null;
    }

    #endregion

    /// <summary>
    /// Обработка очередного принятого кадра
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected async Task ProcessRxQueue(CancellationToken ct)
    {
        var cnt = RxQueue.Reader.Count;

        while (cnt != 0)
        {
            cnt--;

            var rxPacket = await RxQueue.Reader.ReadAsync(ct);

            var apci_2 = MemoryMarshal.AsRef<APCI>(rxPacket.AsSpan(0, APCI.Size));

            if (apci_2.TryGetIPacket(out var iPacket))
            {
                Diagnostic.RcvIPacket(Id);
                await ProcessIPacket(iPacket.Rx, iPacket.Tx, rxPacket[6..], ct);
            }
            else if (apci_2.TryGetUPacket(out var uPacket))
            {
                Diagnostic.RcvUPacket(Id);
                await ProcessUPacket(uPacket.UControl, ct);
            }
            else if (apci_2.TryGetSPacket(out var sPacket))
            {
                Diagnostic.RcvSPacket(Id);
                await ProcessSPacket(sPacket.Rx, ct);
            }
            else
            {
                Logger.LogTrace("Undefined packet rcv");
                Diagnostic.ProtocolError(Id);
                PhysicalLayerController.DisconnectLayer();
                return;
            }
        }
    }

    /// <summary>
    /// Исполнение внешних воздействий
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected abstract Task WorkCycleAsync(CancellationToken ct);

    /// <summary>
    /// Подключение физического уровня
    /// </summary>
    void IPhysicalLayerNotification.Connect()
    {
        Events.Writer.TryWrite(ConnectEvent);
        // запускаем рабочий поток
        WorkCycle ??= Task.Run(() => WorkCycleAsync(Cts.Token));
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
    void IChannelLayerPacketSender.Send(ReadOnlySpan<byte> packet, byte itemCnt, ChannelLayerPacketPriority priority)
    {
        Diagnostic.AppMsgTotal(Id, itemCnt, priority);

        TxQueue.Writer.TryWrite(new SendMsg(packet, itemCnt, priority));
        Events.Writer.TryWrite(TxEvent);
    }

    void IChannelLayerPacketSender.Send(byte[] packet, byte itemCnt, ChannelLayerPacketPriority priority)
    {
        Diagnostic.AppMsgTotal(Id, itemCnt, priority);

        TxQueue.Writer.TryWrite(new SendMsg(packet, itemCnt, priority));
        Events.Writer.TryWrite(TxEvent);
    }

    void ITimeoutOwner.NotifyTimeoutReady(long timeout) => Events.Writer.TryWrite(new TimerEvent(timeout));

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

        Logger.LogProcessRecievedAck(dountAckPacket, ackCount);

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
        Logger.LogProcessIPacket(rx, tx);

        var txDelta = CalcUnAckPacket(_txCounter, rx);
        var counterOk = txDelta >= 0 && txDelta <= _txW && tx == _rxCounter;

        if (counterOk)
        {
            _rxCounter = CounterIncrement(_rxCounter);    // Принятый пакет
            _rxW++;  // не сквитированный пакет

            await ProcessRecievedAck(txDelta, ct);

            if (_rxW >= ChannelLayerModel.WindowWSize)
            {
                new APCI(PacketS.Size, new PacketS(_rxCounter)).SerializeUnsafe(_buffer, 0);
                PhysicalLayerController.SendPacket(_buffer, 0, APCI.Size);
                Diagnostic.SendSPacket(Id);
                _rxW = 0; // несквитированные мною

                Logger.LogProcessIPacket2(_rxCounter);
                // остановить таймер 2
                _timeout2Id = await StopTimerAsync(_timeout2Id, ct);
            }
            else
            {
                // перезапуск таймера 2
                _timeout2Id = await StartTimerAsync(_timeout2Id, TimeSpan.FromSeconds(ChannelLayerModel.Timeout2Sec), ct);
            }

            if (_asduNotification != null)
                ParserGenerator.Parse(_asduNotification, data, _timeProvider.GetUtcNow().DateTime, IsServerSide);
        }
        else
        {
            // нарушение протокола - разрыв соединения по нашей инициативе
            Logger.LogTrace("Disconnect by IPacket Rcv corruption");
            Diagnostic.ProtocolError(Id);
            PhysicalLayerController.DisconnectLayer();
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
        Logger.LogProcessSPacket(rx);

        var txDelta = CalcUnAckPacket(_txCounter, rx);
        var counterOk = txDelta >= 0 && txDelta <= _txW;

        if (counterOk)
        {
            await ProcessRecievedAck(txDelta, ct);
        }
        else
        {
            // нарушение протокола - разрыв соединения по нашей инициативе
            Logger.LogTrace("Disconnect by SPacket Rcv corruption");
            Diagnostic.ProtocolError(Id);
            PhysicalLayerController.DisconnectLayer();
        }
    }

    #endregion

    #region Обработка передаваемых пакетов

    private readonly NetCoreServer.Buffer _largeBuffer = new NetCoreServer.Buffer();

    /// <summary>
    /// Передача пользовательских пакетов
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected async Task ProcessTxQueue(CancellationToken ct)
    {
        // только на установленном соединении
        if (!_isEstablishedConnection)
        {
            Logger.LogTrace("ProcessTxQueue connection not established");
            return;
        }

        // забираем в очередь передачи
        var txQueueCount = TxQueue.Reader.Count;
        while (txQueueCount > 0)
        {
            var sendMsg = await TxQueue.Reader.ReadAsync(ct);
            txQueueCount--;
            if (sendMsg.Priority == ChannelLayerPacketPriority.Low && TxButNotAckQueue.Count > ChannelLayerModel.MaxQueueSize)
            {
                Diagnostic.AppMsgSkip(Id, sendMsg.ItemCnt, sendMsg.Priority);
                continue;
            }
            else
            {
                TxButNotAckQueue.AddLast(sendMsg);
                Diagnostic.AppMsgSend(Id, sendMsg.ItemCnt, sendMsg.Priority);
            }
        }

        // только если позволяет окно
        if (_txW >= ChannelLayerModel.WindowKSize)
        {
            return;
        }

        var packetToSendCount = TxButNotAckQueue.Count;

        if (packetToSendCount > 0)
        {
            packetToSendCount = Math.Min(packetToSendCount, ChannelLayerModel.WindowKSize - _txW);

            var skip = _txW;
            var txPacket = TxButNotAckQueue.First;
            while (skip > 0)
            {
                txPacket = txPacket!.Next;
                skip--;
            }

            _largeBuffer.Clear();

            while (packetToSendCount > 0 && _txW < TxButNotAckQueue.Count)
            {
                var msg = txPacket!.ValueRef;

                var tx = _txCounter << 1;
                var rx = _rxCounter << 1;

                _buffer[0] = APCI.START_PACKET;
                _buffer[1] = (byte)(PacketI.Size + msg.MsgLength);
                _buffer[2] = (byte)(tx & 0xFF);
                _buffer[3] = (byte)(tx >> 8);
                _buffer[4] = (byte)(rx & 0xFF);
                _buffer[5] = (byte)(rx >> 8);

                // Хорошая задумка, но похоже некоторые клиенты могут ожидать ответ только в одном пакете не получив его, они вполне могут разорвать связь поэтому вынесем в тонкую настройку
                //if (_serverModel.ChannelLayerModel.UseFragmentSend)
                //{
                //_physicalLayerController.SendPacket(_buffer, 0, APCI.Size);
                _largeBuffer.Append(_buffer.AsSpan(0, APCI.Size));
                //_physicalLayerController.SendPacket(msg.Msg, 0, msg.MsgLength);
                _largeBuffer.Append(msg.Msg.AsSpan(0, msg.MsgLength));
                //}
                //else
                //{
                //    msg.Msg.AsSpan(0, msg.MsgLength).CopyTo(_buffer.AsSpan(APCI.Size));
                //    _physicalLayerController.SendPacket(_buffer, 0, APCI.Size + msg.MsgLength);
                //}

                Diagnostic.SendIPacket(Id);
                Logger.LogProcessTxQueue(_rxCounter, _txCounter);

                _rxW = 0; // сброс счетчика несквитированных посылок
                _txCounter = CounterIncrement(_txCounter); // наращиваем счетчик отправленных
                _txW++; // наращиваем счетчик не квитированных
                packetToSendCount--;
                txPacket = txPacket.Next;
            }

            PhysicalLayerController.SendPacket(_largeBuffer.Data, 0, _largeBuffer.Size);

            // остановить таймер 2
            _timeout2Id = await StopTimerAsync(_timeout2Id, ct);
            // перезапуск таймера 1
            _timeout1Id = await StartTimerAsync(_timeout1Id, TimeSpan.FromSeconds(ChannelLayerModel.Timeout1Sec), ct);
        }
    }

    #endregion

    protected abstract Task ProcessUPacket(UControl control, CancellationToken ct);
    public void Dispose()
    {
        (_asduNotification as IDisposable)?.Dispose();
        (_timeoutsService as IDisposable)?.Dispose();
        Cts.Cancel();
        Cts.Dispose();
    }

    protected abstract string Id { get; }
    protected abstract IEC104ChannelLayerModel ChannelLayerModel { get; }
    protected abstract bool IsServerSide { get; }

    protected readonly struct SendMsg
    {
        public readonly byte[] Msg;
        public readonly byte ItemCnt;
        public readonly int MsgLength;
        public readonly ChannelLayerPacketPriority Priority;
        private readonly bool _usePool;
        public SendMsg(ReadOnlySpan<byte> packet, byte itemCnt, ChannelLayerPacketPriority priority)
        {
            MsgLength = packet.Length;
            ItemCnt = itemCnt;
            Msg = ArrayPool<byte>.Shared.Rent(packet.Length);
            packet.CopyTo(Msg);
            Priority = priority;
            _usePool = true;
        }

        public SendMsg(byte[] packet, byte itemCnt, ChannelLayerPacketPriority priority)
        {
            MsgLength = packet.Length;
            ItemCnt = itemCnt;
            Msg = packet;
            Priority = priority;
        }

        public void Dispose()
        {
            if (_usePool)
                ArrayPool<byte>.Shared.Return(Msg);
        }
    }
}
