using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PowerUnit;

public sealed class IEC104ServersStarterService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IEC104ServersStarterService> _logger;
    private readonly List<IEC104Server> _servers = [];
    public IEC104ServersStarterService(IServiceProvider serviceProvider, ILogger<IEC104ServersStarterService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            var iec60870_5_104ServerFactory = _serviceProvider.GetRequiredService<IEC104ServerFactory>();

            try
            {
                var server = iec60870_5_104ServerFactory.CreateServer(new IEC104ServerModel() { });
                _servers.Add(server);
                server.Start();
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
