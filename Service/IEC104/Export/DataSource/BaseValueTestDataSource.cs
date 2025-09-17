using Microsoft.Extensions.Logging;

namespace PowerUnit.Service.IEC104.Export.DataSource;

internal sealed class BaseValueTestDataSource : TestDataSource<BaseValue>
{
    private const int COUNT = 1024;
    private readonly BaseValue[] _testDataAnalog;
    private readonly BaseValue[] _testDataDiscret;

    public BaseValueTestDataSource(TimeProvider timeProvider, ILogger<BaseValueTestDataSource> logger) : base(timeProvider, logger)
    {
        _testDataAnalog = new BaseValue[COUNT];
        _testDataDiscret = new BaseValue[COUNT];
        for (var i = 0; i < _testDataAnalog.Length; i++)
        {
            var now = DateTime.UtcNow;
            var randomValue = Random.Shared.NextDouble();
            _testDataAnalog[i] = new BaseValue()
            {
                SourceId = "1",
                EquipmentId = Random.Shared.NextInt64(1, 4).ToString(),
                ParameterId = Random.Shared.NextInt64(1, 4).ToString(),
                ValueAsFloat = (float)randomValue
            };
            _testDataDiscret[i] = new BaseValue()
            {
                SourceId = "1",
                EquipmentId = Random.Shared.NextInt64(1, 4).ToString(),
                ParameterId = Random.Shared.NextInt64(11, 14).ToString(),
                ValueAsBool = randomValue < 0.5
            };
        }
    }

    protected override BaseValue CreateNewValue(DateTime now)
    {
        BaseValue value;
        var randomType = Random.Shared.NextDouble();
        var randomValue = Random.Shared.Next(0, COUNT);

        if (randomType < 1.5)
        {
            value = _testDataAnalog[randomValue];
        }
        else
        {
            value = _testDataDiscret[randomValue];
        }

        value.ValueDt = now;
        value.RegistrationDt = now;

        return value;
    }
}
