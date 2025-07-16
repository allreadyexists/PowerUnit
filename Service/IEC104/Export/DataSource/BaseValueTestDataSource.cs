namespace PowerUnit.Service.IEC104.Export.DataSource;

internal sealed class BaseValueTestDataSource : TestDataSource<BaseValue>
{
    public BaseValueTestDataSource(TimeProvider timeProvider, ILogger<BaseValueTestDataSource> logger) : base(timeProvider, logger)
    { }

    protected override BaseValue CreateNewValue(DateTime now)
    {
        var randomType = Random.Shared.NextDouble();
        var randomValue = Random.Shared.NextDouble();
        if (randomType < 0.5)
        {
            return new AnalogValue(Random.Shared.NextInt64(1, 2), Random.Shared.NextInt64(1, 5), (float)randomValue, now, now);
        }
        else
        {
            return new DiscretValue(Random.Shared.NextInt64(2, 3), Random.Shared.NextInt64(1000, 1002), randomValue < 0.5, now, now);
        }
    }
}
