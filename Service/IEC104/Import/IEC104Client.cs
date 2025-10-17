using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Channel;

using System.Net;
using System.Net.Sockets;

namespace PowerUnit.Service.IEC104.Import;

public class IEC104Client : NetCoreServer.TcpClient, IPhysicalLayerCommander
{
    private readonly IEC60870_5_104ClientChannelLayer _channelLayer;
    private readonly ILogger<IEC104Client> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public IEC104Client(IPAddress address, int port, IServiceProvider provider, ILogger<IEC104Client> logger) : base(address, port)
    {
        _logger = logger;
        // Канальный уровень
        _channelLayer = ActivatorUtilities.CreateInstance<IEC60870_5_104ClientChannelLayer>(provider,
            [$"{address}:{port}", this, _cancellationTokenSource.Token]);
    }

    protected override void OnConnected()
    {
        ((IPhysicalLayerNotification)_channelLayer).Connect();
    }

    protected override void OnDisconnected()
    {
        ((IPhysicalLayerNotification)_channelLayer).Disconnect();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var bufferPart = buffer[(int)offset..(int)(offset + size)];
        ((IPhysicalLayerNotification)_channelLayer).Recieve(bufferPart);
    }

    protected override void OnError(SocketError error)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogError("{@id} {@error}", Id, error);
    }

    bool IPhysicalLayerCommander.SendPacket(byte[] buffer, long offset, long size)
    {
        var result = base.Send(new ReadOnlySpan<byte>(buffer, (int)offset, (int)size));
        return result > 0;
    }

    bool IPhysicalLayerCommander.DisconnectLayer() => Disconnect();
}

