using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCoreServer;

using PowerUnit.Common.StringHelpers;
using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Channel;
using PowerUnit.Service.IEC104.Models;

using System.Net.Sockets;

namespace PowerUnit.Service.IEC104.Export;

public class IEC104ServerSession : TcpSession, IPhysicalLayerCommander
{
    private readonly IEC104ServerModel _options;

    private readonly IEC60870_5_104ServerChannelLayer _channelLayer;
    private readonly ILogger<IEC104ServerSession> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public IEC104ServerSession(IServiceProvider provider, IEC104ServerModel options, IDataSource<MapValueItem> dataSource,
        IDataProvider dataProvider,
        TcpServer server, ILogger<IEC104ServerSession> logger) : base(server)
    {
        OptionSendBufferSize = 256 * 3 / 2;
        OptionReceiveBufferSize = 256 * 3 / 2;

        _options = options;

        _logger = logger;

        // Канальный уровень
        _channelLayer = ActivatorUtilities.CreateInstance<IEC60870_5_104ServerChannelLayer>(provider,
            [this, _options, dataSource, dataProvider, _cancellationTokenSource.Token]);
    }

    protected override void OnConnecting()
    {
        _logger.LogTrace("{@id} connect", Id);
        ((IPhysicalLayerNotification)_channelLayer).Connect();
    }

    protected override void OnDisconnecting()
    {
        _logger.LogTrace("{@id} disconnect", Id);
        (_channelLayer as IPhysicalLayerNotification)?.Disconnect();
        _cancellationTokenSource.Cancel();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var bufferPart = buffer[(int)offset..(int)(offset + size)];
        _logger.LogTrace("{@id} Rx: {@rx}", Id, new Span<byte>(bufferPart, (int)offset, (int)size).ToHex());
        ((IPhysicalLayerNotification)_channelLayer).Recieve(bufferPart);
    }

    public sealed override long Send(byte[] buffer, long offset, long size)
    {
        var result = base.Send(buffer, offset, size);
        _logger.LogTrace("{@id} Tx: {@tx}", Id, new Span<byte>(buffer, (int)offset, (int)size).ToHex());
        return result;
    }

    protected override void OnError(SocketError error)
    {
        _logger.LogError("{@id} {@error}", Id, error);
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
