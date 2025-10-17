using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Net;

namespace PowerUnit.Service.IEC104.Import;

public sealed class IEC104ClientsStarterService : BackgroundService
{
    private readonly IEC104Client[] _clients;

    public IEC104ClientsStarterService(IServiceProvider serviceProvider)
    {
        var ipAddress = IPAddress.Parse("127.0.0.1");
        _clients = [.. Enumerable.Range(0, 1).Select(x => ActivatorUtilities.CreateInstance<IEC104Client>(serviceProvider, [ipAddress, 2404 + x]))];
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var i = 0; i < _clients.Length; i++)
        {
            try
            {
                _clients[i].ConnectAsync();
                while (!_clients[i].IsConnected)
                    Thread.Yield();
            }
            catch (Exception ex)
            {

            }
        }

        return Task.CompletedTask;
    }
}

