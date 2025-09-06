using Microsoft.Extensions.Logging;

namespace PowerUnit.Service.IEC104.Export;

internal static partial class IEC104ServersStarterServiceLogExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Start IEC104 server \"{ServerName}\" on port: {Port}")]
    public static partial void LogServerStart(this ILogger logger, string serverName, int port);
}
