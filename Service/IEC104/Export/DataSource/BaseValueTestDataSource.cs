using Microsoft.Extensions.Logging;

namespace PowerUnit.Service.IEC104.Export.DataSource;

internal sealed class BaseValueTestDataSource : TestDataSource<BaseValue>
{
    public BaseValueTestDataSource(TimeProvider timeProvider, ILogger<BaseValueTestDataSource> logger) : base(timeProvider, logger)
    {
    }

    protected override BaseValue CreateNewValue(DateTime now)
    {
        var randomType = Random.Shared.NextDouble();
        var randomValue = Random.Shared.NextDouble();
        if (randomType < 1.5)
        {
            return new BaseValue("1", Random.Shared.NextInt64(1, 4).ToString(), Random.Shared.NextInt64(1, 4).ToString(), now, now, (float)randomValue, null);
        }
        else
        {
            return new BaseValue("1", Random.Shared.NextInt64(1, 4).ToString(), Random.Shared.NextInt64(10, 14).ToString(), now, now, null, randomValue < 0.5);
        }
    }
}
