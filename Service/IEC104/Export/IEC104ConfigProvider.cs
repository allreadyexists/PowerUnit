using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;

using PowerUnit.Infrastructure.IEC104ServerDb;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Models;

namespace PowerUnit.Service.IEC104.Export;

internal sealed class IEC104ConfigProvider : IConfigProvider
{
    private static readonly AsyncRetryPolicy _policyReadServersSettings = Policy.Handle<Exception>().WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(30));
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IEC104ConfigProvider> _logger;
    public IEC104ConfigProvider(IServiceProvider serviceProvider, ILogger<IEC104ConfigProvider> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    Task<IEC104ServerModel[]> IConfigProvider.GetServersAsync(CancellationToken stoppingToken)
    {
        return _policyReadServersSettings.ExecuteAsync(async (ct) =>
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<IPowerUnitIEC104ServerDbContext>();
#pragma warning disable IDE0100 // Remove redundant equality
            var servers = await dbContext.Servers.AsNoTracking().Where(x => x.Enable == true)
                .Include(x => x.ChannelLayerOption)
                .Include(x => x.ApplicationLayerOption).OrderBy(x => x.Id)
                .ToArrayAsync(ct);
#pragma warning restore IDE0100 // Remove redundant equality

            foreach (var server in servers)
            {
                if (server.ChannelLayerOption == null)
                    server.ChannelLayerOption = new IEC104ServerChannelLayerOptionItem();
                if (server.ApplicationLayerOption == null)
                    server.ApplicationLayerOption = new IEC104ServerApplicationLayerOptionItem();
            }

            return servers.Select(x =>
            {
                if (x.ChannelLayerOption!.WindowWSize > 2.0 * x.ChannelLayerOption!.WindowKSize / 3)
                    _logger.LogWarning(@"Check server {Id} settings: {WindowWSize} greate than 2/3 {WindowKSize}", x.Id, x.ChannelLayerOption!.WindowWSize, x.ChannelLayerOption!.WindowKSize);

                if (x.ChannelLayerOption!.MaxQueueSize < x.ChannelLayerOption!.WindowKSize)
                    _logger.LogWarning(@"Check server {Id} settings: {MaxQueueSize} less than {WindowKSize}", x.Id, x.ChannelLayerOption!.MaxQueueSize, x.ChannelLayerOption!.WindowKSize);

                return new IEC104ServerModel()
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
                        UseFragmentSend = x.ChannelLayerOption!.UseFragmentSend,
                        MaxQueueSize = x.ChannelLayerOption!.MaxQueueSize
                    }
                };
            }).ToArray();
        }, stoppingToken);
    }

    Task<IEC104MappingModel[]> IConfigProvider.GetMappingModelsAsync(CancellationToken stoppingToken)
    {
        return _policyReadServersSettings.ExecuteAsync(async (ct) =>
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<IPowerUnitIEC104ServerDbContext>();
            var result = await dbContext.Mappings.Where(x => x.SourceId != "").AsNoTracking().Join(dbContext.Groups.AsNoTracking(),
                f => f.Id,
                s => s.MappingId,
                (f, s) => new IEC104MappingModel()
                {
                    ServerId = f.ServerId,
                    Group = s.Group,
                    Address = f.Address,
                    AsduType = (byte)f.TypeId,
                    SourceId = f.SourceId,
                    EquipmentId = f.EquipmentId,
                    ParameterId = f.ParameterId,
                }).ToArrayAsync(ct);
            return result;
        }, stoppingToken);
    }
}

