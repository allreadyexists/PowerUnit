using PowerUnit.Service.IEC104.Abstract;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104ServersStarterService : BackgroundService
{
    private readonly IConfigProvider _configProvider;
    private readonly IEC104ServerFactory _iec104ServerFactory;
    private readonly ILogger<IEC104ServersStarterService> _logger;
    private readonly List<IEC104Server> _servers = [];
    public IEC104ServersStarterService(IConfigProvider configProvider, IEC104ServerFactory iec104ServerFactory, ILogger<IEC104ServersStarterService> logger)
    {
        _configProvider = configProvider;
        _iec104ServerFactory = iec104ServerFactory;
        _logger = logger;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            var mappings = (await _configProvider.GetMappingModelsAsync(stoppingToken)).ToLookup(x => x.ServerId);

            foreach (var serverModel in await _configProvider.GetServersAsync(stoppingToken))
            {
                try
                {
                    var server = _iec104ServerFactory.CreateServer(serverModel, mappings[serverModel.ApplicationLayerModel.ServerId]);
                    server.Start();
                    _servers.Add(server);
                    _logger.LogServerStart(serverModel.ServerName, serverModel.Port);
                }
                catch (ObjectDisposedException disposeException)
                {
                    _logger.LogError(disposeException, "Start Object Disposed Exception");
                }
                catch (OperationCanceledException cancelException)
                {
                    _logger.LogError(cancelException, "Start Operation Canceled Exception");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Start Exception");
                }
            }
        }, stoppingToken);
    }

    public override void Dispose()
    {
        foreach (var server in _servers)
        {
            try
            {
                server.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stop Exception");
            }
        }

        base.Dispose();
    }
}
