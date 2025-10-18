using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Channel;

using System.Net;
using System.Net.Sockets;

namespace PowerUnit.Service.IEC104.Import;

internal sealed class IEC104BaseClient : NetCoreServer.TcpClient, IPhysicalLayerCommander
{
    private readonly IPAddress _address;
    private readonly int _port;
    private readonly IServiceProvider _provider;
    private IEC60870_5_104ClientChannelLayer? _channelLayer;
    private readonly ILogger<IEC104BaseClient> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Lock _lock = new Lock();
    private Task? _connectedTask;
    private readonly TimeSpan _beforeConnectDelay;
    private readonly TimeSpan _connectRetryPeriod;

    public IEC104BaseClient(IPAddress address, int port, TimeSpan beforeConnectDelay, TimeSpan connectRetryPeriod, IServiceProvider provider, ILogger<IEC104BaseClient> logger) : base(address, port)
    {
        _address = address;
        _port = port;
        _provider = provider;
        _logger = logger;
        _beforeConnectDelay = beforeConnectDelay;
        _connectRetryPeriod = connectRetryPeriod;
    }

    protected override void OnConnected()
    {
        _channelLayer = ActivatorUtilities.CreateInstance<IEC60870_5_104ClientChannelLayer>(_provider,
            [$"{_address}:{_port}", this]);
        ((IPhysicalLayerNotification)_channelLayer).Connect();
        _logger.LogDebug("Connected");
    }

    protected override void OnDisconnected()
    {
        _channelLayer?.Dispose();
        _channelLayer = null;
        _logger.LogDebug("Disconnected");
        StartAsync();
    }

    public void StartAsync()
    {
        lock (_lock)
        {
            if (_connectedTask == null)
            {
                _connectedTask = new Task(static async (state) =>
                {
                    var client = (IEC104BaseClient)state!;
                    var ct = client._cancellationTokenSource.Token;

                    try
                    {

                        await Task.Delay(client._beforeConnectDelay, ct);

                        while (!ct.IsCancellationRequested && !client.ConnectAsync())
                        {
                            try
                            {
                                await Task.Delay(client._connectRetryPeriod, ct);
                                client._logger.LogDebug("Try to connect");
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        client._logger.LogError(ex, "Start error");
                    }

                    client._connectedTask = null;
                }, this);
                _connectedTask.Start();
            }
        }
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var bufferPart = buffer[(int)offset..(int)(offset + size)];
        (_channelLayer as IPhysicalLayerNotification)?.Recieve(bufferPart);
    }

    protected override void OnError(SocketError error)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogError("{@id} {@error}", Id, error);
    }

    bool IPhysicalLayerCommander.SendPacket(byte[] buffer, long offset, long size)
    {
        var result = Send(new ReadOnlySpan<byte>(buffer, (int)offset, (int)size));
        return result > 0;
    }

    bool IPhysicalLayerCommander.DisconnectLayer() => Disconnect();

    protected override void Dispose(bool disposingManagedResources)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _channelLayer?.Dispose();

        base.Dispose(disposingManagedResources);
    }
}

