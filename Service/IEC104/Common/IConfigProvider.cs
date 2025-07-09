using PowerUnit.Service.IEC104.Options.Models;

namespace PowerUnit.Service.IEC104.Abstract;

public interface IConfigProvider
{
    Task<IEC104ServerModel[]> GetServersAsync(CancellationToken stoppingToken);
}
