namespace PowerUnit;

public interface IConfigProvider
{
    Task<IEC104ServerModel[]> GetServersAsync(CancellationToken stoppingToken);
}
