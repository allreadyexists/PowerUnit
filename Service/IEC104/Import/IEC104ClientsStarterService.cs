using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Net;

namespace PowerUnit.Service.IEC104.Import;

public sealed class IEC104ClientsStarterService : BackgroundService
{
    private readonly IEC104BaseClient[] _clients;

    public IEC104ClientsStarterService(IServiceProvider serviceProvider)
    {
        var ipAddress = IPAddress.Parse("192.168.1.69");
        _clients = [.. Enumerable.Range(0, 1).Select(x => ActivatorUtilities.CreateInstance<IEC104BaseClient>(serviceProvider,
            [ipAddress, 2404, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)]
            ))];
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var i = 0; i < _clients.Length; i++)
        {
            try
            {
                _clients[i].StartAsync();
            }
            catch (Exception ex)
            {

            }
        }

        return Task.CompletedTask;
    }
}

