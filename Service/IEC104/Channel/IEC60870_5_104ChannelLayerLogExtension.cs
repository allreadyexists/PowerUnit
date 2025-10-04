using Microsoft.Extensions.Logging;

using PowerUnit.Service.IEC104.Channel.Events;
using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104.Channel;

internal static partial class IEC60870_5_104ChannelLayerLogExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "TX: S: RX: {RxCounter}")]
    public static partial void LogTimer2(this ILogger logger, ushort rxCounter);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "TX: U: {Test}")]
    public static partial void LogTimer3(this ILogger logger, UControl test);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "TX: I: RX: {RxCounter}, TX: {TxCounter}")]
    public static partial void LogProcessTxQueue(this ILogger logger, ushort rxCounter, ushort txCounter);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "ProcessRecievedAck: dountAckPacket = {DountAckPacket}, ackCount = {AckCount}")]
    public static partial void LogProcessRecievedAck(this ILogger logger, int dountAckPacket, int ackCount);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "RX: I: RX {Rx}, TX: {Tx}")]
    public static partial void LogProcessIPacket(this ILogger logger, ushort rx, ushort tx);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "TX: S: RX:{Rx}")]
    public static partial void LogProcessIPacket2(this ILogger logger, ushort rx);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "RX: S: RX {Rx}")]
    public static partial void LogProcessSPacket(this ILogger logger, ushort rx);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "RX: U: {Control}")]
    public static partial void LogProcessUPacket(this ILogger logger, UControl control);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "Event: {evnt} {@timerId:X2}", SkipEnabledCheck = true)]
    public static partial void LogTimerEvent(this ILogger logger, IEvent evnt, long timerId);

    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "Event: {evnt}", SkipEnabledCheck = true)]
    public static partial void LogEvent(this ILogger logger, IEvent evnt);
}

