namespace PowerUnit.Service.IEC104.Export.DataSource;

internal sealed class BaseValueTestDataSource : TestDataSource<BaseValue>
{
    private bool _toggle;

    public BaseValueTestDataSource(TimeProvider timeProvider, ILogger<BaseValueTestDataSource> logger) : base(timeProvider, logger)
    {
    }

    protected override BaseValue CreateNewValue(DateTime now)
    {
        try
        {
            var randomType = Random.Shared.NextDouble();
            var randomValue = Random.Shared.NextDouble();
            if (_toggle)
            {
                return new AnalogValue("1", Random.Shared.NextInt64(1, 4).ToString(), Random.Shared.NextInt64(1, 5).ToString(), (float)randomValue, now, now);
            }
            else
            {
                return new DiscretValue("1", Random.Shared.NextInt64(1, 4).ToString(), Random.Shared.NextInt64(1000, 1002).ToString(), randomValue < 0.5, now, now);
            }
        }
        finally
        {
            _toggle = !_toggle;
        }
    }
}
