using Microsoft.EntityFrameworkCore;

using Polly;
using Polly.Retry;

using PowerUnit.Infrastructure.IEC104ServerDb;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Options.Models;

namespace PowerUnit.Service.IEC104.Export;

internal sealed class ConfigProvider : IConfigProvider
{
    private static readonly AsyncRetryPolicy _policyReadServersSettings = Policy.Handle<Exception>().WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(30));
    private readonly IServiceProvider _serviceProvider;
    public ConfigProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    Task<IEC104ServerModel[]> IConfigProvider.GetServersAsync(CancellationToken stoppingToken)
    {
        return _policyReadServersSettings.ExecuteAsync(async (context) =>
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<PowerUnitIEC104ServerDbContext>();
            var servers = await dbContext.IEC104Servers.AsNoTracking().Where(x => x.Enable)
                .Include(x => x.ChannelLayerOption)
                .Include(x => x.ApplicationLayerOption).OrderBy(x => x.Id)
                .ToArrayAsync(stoppingToken);

            foreach (var server in servers)
            {
                if (server.ChannelLayerOption == null)
                    server.ChannelLayerOption = new IEC104ServerChannelLayerOptionItem();
                if (server.ApplicationLayerOption == null)
                    server.ApplicationLayerOption = new IEC104ServerApplicationLayerOptionItem();
            }

            return servers.Select(x => new IEC104ServerModel()
            {
                ServerName = x.Name,
                Port = x.Port,
                ApplicationLayerModel = new IEC104ApplicationLayerModel()
                {
                    ServerId = x.Id,
                    CommonASDUAddress = x.CommonASDUAddress,
                    CheckCommonASDUAddress = x.ApplicationLayerOption!.CheckCommonASDUAddress,
                    SporadicSendEnabled = x.ApplicationLayerOption!.SporadicSendEnabled
                },
                ChannelLayerModel = new IEC104ChannelLayerModel()
                {
                    Timeout0Sec = x.ChannelLayerOption!.Timeout0Sec,
                    Timeout1Sec = x.ChannelLayerOption!.Timeout1Sec,
                    Timeout2Sec = x.ChannelLayerOption!.Timeout2Sec,
                    Timeout3Sec = x.ChannelLayerOption!.Timeout3Sec,
                    WindowKSize = x.ChannelLayerOption!.WindowKSize,
                    WindowWSize = x.ChannelLayerOption!.WindowWSize,
                    UseFragmentSend = x.ChannelLayerOption!.UseFragmentSend
                }
            }).ToArray();
        }, stoppingToken);


    }
}

