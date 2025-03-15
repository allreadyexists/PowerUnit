using PowerUnit.Service.IEC104.Models;

namespace PowerUnit.Service.IEC104.Abstract;

public interface IConfigProvider
{
    Task<IEC104ServerModel[]> GetServersAsync(CancellationToken stoppingToken);

    Task<IEC104MappingModel[]> GetMappingModelsAsync(CancellationToken stoppingToken);
}
