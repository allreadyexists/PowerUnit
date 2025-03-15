using Microsoft.Extensions.Logging;

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
}

