using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Channel.Events;

using System.Buffers;
using System.Threading.Channels;

namespace PowerUnit.Service.IEC104.Channel;

public abstract class IEC60870_5_104ChannelLayer
{
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
    protected List<SendMsg> TxButNotAckQueue { get; } = [];

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

    protected readonly struct SendMsg
    {
        public readonly byte[] Msg;
        public readonly int MsgLength;
        public readonly ChannelLayerPacketPriority Priority;
        private readonly bool _usePool;
        public SendMsg(ReadOnlySpan<byte> packet, ChannelLayerPacketPriority priority)
        {
            MsgLength = packet.Length;
            Msg = ArrayPool<byte>.Shared.Rent(packet.Length);
            packet.CopyTo(Msg);
            Priority = priority;
            _usePool = true;
        }

        public SendMsg(byte[] packet, ChannelLayerPacketPriority priority)
        {
            MsgLength = packet.Length;
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

