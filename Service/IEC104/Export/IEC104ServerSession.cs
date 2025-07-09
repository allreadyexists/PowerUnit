using NetCoreServer;

using PowerUnit.Common.StringHelpers;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Channel;
using PowerUnit.Service.IEC104.Options.Models;

using System.Net.Sockets;

namespace PowerUnit.Service.IEC104.Export;

public class IEC104ServerSession : TcpSession, IPhysicalLayerCommander
{
    private readonly IServiceProvider _provider;
    private readonly IEC104ServerModel _options;

    private readonly Iec60870_5_104ServerChannelLayer _channelLayer;
    private readonly ILogger<IEC104ServerSession> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public IEC104ServerSession(IServiceProvider provider, IEC104ServerModel options, TcpServer server) : base(server)
    {
        OptionSendBufferSize = 256 * 3 / 2;
        OptionReceiveBufferSize = 256 * 3 / 2;

        _provider = provider;
        _options = options;

        _logger = _provider.GetRequiredService<ILogger<IEC104ServerSession>>();

        // Канальный уровень
        _channelLayer = new Iec60870_5_104ServerChannelLayer(_provider, this, _options, _provider.GetRequiredService<ILogger<Iec60870_5_104ServerChannelLayer>>(), _cancellationTokenSource.Token);
    }

    protected override void OnConnecting()
    {
        _logger.LogTrace($"{Id} connect");
        ((IPhysicalLayerNotification)_channelLayer).Connect();
    }

    protected override void OnDisconnecting()
    {
        _logger.LogTrace($"{Id} disconnect");
        (_channelLayer as IPhysicalLayerNotification)?.Disconnect();
        _cancellationTokenSource.Cancel();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var bufferPart = buffer[(int)offset..(int)(offset + size)];
        _logger.LogTrace($"{Id} Rx: {new Span<byte>(bufferPart, (int)offset, (int)size).ToHex()}");
        ((IPhysicalLayerNotification)_channelLayer).Recieve(bufferPart);
    }

    public sealed override long Send(byte[] buffer, long offset, long size)
    {
        var result = base.Send(buffer, offset, size);
        _logger.LogTrace($"{Id} Tx: {new Span<byte>(buffer, (int)offset, (int)size).ToHex()}");
        return result;
    }

    protected override void OnError(SocketError error)
    {
        _logger.LogError($"{Id} {error}");
    }

    long IPhysicalLayerCommander.SendPacket(byte[] buffer, long offset, long size)
    {
        return Send(buffer, offset, size);
    }

    bool IPhysicalLayerCommander.DisconnectLayer()
    {
        return Disconnect();
    }
}
