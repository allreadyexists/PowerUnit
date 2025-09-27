using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PowerUnit.Service.IEC104.Export.DataSource;

internal sealed class BaseValueTestDataSource : TestDataSource<BaseValue>
{
    private const int COUNT = 1024;
    private readonly BaseValue[] _testDataAnalog;
    private readonly BaseValue[] _testDataDiscret;
    private bool _toggler;
    private readonly GenerateMode _mode;

    private enum GenerateMode
    {
        Single,
        Random,
        Block
    }

    public BaseValueTestDataSource(TimeProvider timeProvider, IConfiguration configuration, ILogger<BaseValueTestDataSource> logger) : base(timeProvider, logger)
    {
        _mode = configuration.GetValue("GenerateMode", GenerateMode.Single);
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

    private BaseValue CreateNewValueSingleMode(DateTime now)
    {
        var randomValue = Random.Shared.Next(0, COUNT);

        BaseValue value = _toggler ? _testDataAnalog[randomValue] : _testDataDiscret[randomValue];
        value.ValueDt = now;
        value.RegistrationDt = now;

        _toggler = !_toggler;

        return value;
    }
    private BaseValue CreateNewValueRandomMode(DateTime now)
    {
        BaseValue value;
        var randomType = Random.Shared.NextDouble();
        var randomValue = Random.Shared.Next(0, COUNT);

        value = randomType < 0.5 ? _testDataAnalog[randomValue] : _testDataDiscret[randomValue];
        value.ValueDt = now;
        value.RegistrationDt = now;

        return value;
    }
    private BaseValue CreateNewValueBlockMode(DateTime now)
    {
        var randomValue = Random.Shared.Next(0, COUNT);

        BaseValue value = _testDataAnalog[randomValue];
        value.ValueDt = now;
        value.RegistrationDt = now;

        return value;
    }

    protected override BaseValue CreateNewValue(DateTime now)
    {
        return _mode switch
        {
            GenerateMode.Single => CreateNewValueSingleMode(now),
            GenerateMode.Random => CreateNewValueRandomMode(now),
            GenerateMode.Block => CreateNewValueBlockMode(now),
            _ => throw new NotImplementedException(),
        };
    }
}
