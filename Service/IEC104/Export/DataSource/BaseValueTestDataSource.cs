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
            return new AnalogValue("1", Random.Shared.NextInt64(1, 2).ToString(), Random.Shared.NextInt64(1, 5).ToString(), (float)randomValue, now, now);
        }
        else
        {
            return new DiscretValue("1", Random.Shared.NextInt64(2, 3).ToString(), Random.Shared.NextInt64(1000, 1002).ToString(), randomValue < 0.5, now, now);
        }
    }
}
