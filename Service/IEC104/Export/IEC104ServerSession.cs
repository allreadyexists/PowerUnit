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
    private readonly IServiceProvider _provider;
    private readonly IEC104ServerModel _options;
    private readonly IDataSource<MapValueItem> _dataSource;
    private readonly IDataProvider _dataProvider;

    private IEC60870_5_104ServerChannelLayer? _channelLayer;
    private readonly ILogger<IEC104ServerSession> _logger;

    public IEC104ServerSession(IServiceProvider provider, IEC104ServerModel options, IDataSource<MapValueItem> dataSource,
        IDataProvider dataProvider,
        TcpServer server, ILogger<IEC104ServerSession> logger) : base(server)
    {
        //OptionSendBufferSize = 256 * 3 / 2;
        //OptionReceiveBufferSize = 256 * 3 / 2;

        _provider = provider;
        _options = options;
        _dataSource = dataSource;
        _dataProvider = dataProvider;

        _logger = logger;
    }

    protected override void OnConnecting()
    {
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogTrace("{@id} connect", Id);
        _channelLayer = ActivatorUtilities.CreateInstance<IEC60870_5_104ServerChannelLayer>(_provider,
            [this, _options, _dataSource, _dataProvider]);
        ((IPhysicalLayerNotification)_channelLayer).Connect();
    }

    protected override void OnDisconnecting()
    {
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogTrace("{@id} disconnect", Id);
        _channelLayer?.Dispose();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var bufferPart = buffer[(int)offset..(int)(offset + size)];
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogTrace("{@id} Rx: {@rx}", Id, new Span<byte>(bufferPart, (int)offset, (int)size).ToHex());
        ((IPhysicalLayerNotification)_channelLayer).Recieve(bufferPart);
    }

    public bool SendInternal(byte[] buffer, long offset, long size)
    {
        var result = base.Send(new ReadOnlySpan<byte>(buffer, (int)offset, (int)size));
        if (result > 0 && _logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("{@id} Tx: {@tx}", Id, new Span<byte>(buffer, (int)offset, (int)size).ToHex());
        }

        return result > 0;
    }

    protected override void OnError(SocketError error)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogError("{@id} {@error}", Id, error);
    }

    bool IPhysicalLayerCommander.SendPacket(byte[] buffer, long offset, long size)
    {
        return SendInternal(buffer, offset, size);
    }

    bool IPhysicalLayerCommander.DisconnectLayer()
    {
        return Disconnect();
    }

    protected override void Dispose(bool disposingManagedResources)
    {
        _channelLayer?.Dispose();
        base.Dispose(disposingManagedResources);
    }
}
