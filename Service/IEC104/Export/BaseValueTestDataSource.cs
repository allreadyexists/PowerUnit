using PowerUnit.Service.IEC104.Abstract;

namespace PowerUnit.Service.IEC104.Export;

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

//internal sealed class DataProvider : IDataProvider
//{
//    private readonly IServiceProvider _serviceProvider;

//    public DataProvider(IServiceProvider serviceProvider)
//    {
//        _serviceProvider = serviceProvider;
//    }

//    private readonly Func<PowerUnitDbContext, int, byte, IAsyncEnumerable<AnalogValue>> _measurements = EF.CompileAsyncQuery((PowerUnitDbContext dbContext, int serverId, byte qoi) =>
//    dbContext.IEC104Mappings.AsNoTracking().Include(x => x.Equipment).Where(x => x.ServerId == serverId)
//            .Join(dbContext.IEC104Groups.AsNoTracking().Where(x => x.Group == qoi), u => u.Id, v => v.IEC104MappingId, (u, v) =>
//            new
//            {
//                iec104Type = (AsduType)u.IEC104TypeId,
//                address = (ushort)u.Address,
//                equipmentId = u.EquipmentId,
//                parameterTypeId = u.ParameterTypeId
//            })
//            .Join(dbContext.Measurements.AsNoTracking().OrderByDescending(x => x.ValueDt),
//                u => new { eq = u.equipmentId, p = u.parameterTypeId },
//                v => new { eq = v.EquipmentId, p = v.ParameterTypeId },
//                (u, v) => new AnalogValue(u.iec104Type, u.address, (float)v.Value, v.ValueDt, v.RegistrationDt, 0)));

//    private readonly Func<PowerUnitDbContext, int, int, Task<AnalogValue?>> _measurement = EF.CompileAsyncQuery((PowerUnitDbContext dbContext, int serverId, int address) => dbContext.IEC104Mappings.AsNoTracking().Include(x => x.Equipment).Where(x => x.ServerId == serverId && x.Address == address)
//        .Join(dbContext.Measurements.AsNoTracking().OrderByDescending(x => x.ValueDt),
//            u => new { eq = u.EquipmentId, p = u.ParameterTypeId },
//            v => new { eq = v.EquipmentId, p = v.ParameterTypeId },
//            (u, v) => new AnalogValue((AsduType)u.IEC104TypeId, (ushort)u.Address, (float)v.Value, v.ValueDt, v.RegistrationDt, 0)).FirstOrDefault());

//    async IAsyncEnumerable<AnalogValue> IDataProvider.GetAnalogGroup(int serverId, QOI qoi, [EnumeratorCancellation] CancellationToken ct)
//    {
//        using var scope = _serviceProvider.CreateAsyncScope();
//        using var dbContext = scope.ServiceProvider.GetRequiredService<PowerUnitDbContext>();
//        await foreach (var value in _measurements(dbContext, serverId, (byte)qoi))
//        {
//            yield return value;
//        }
//    }

//    async IAsyncEnumerable<DiscretValue/*TODO подумать*/> IDataProvider.GetDiscretGroup(int serverId, QOI qoi, [EnumeratorCancellation] CancellationToken ct)
//    {
//        await Task.Delay(1, ct);
//        yield break;
//    }

//    async Task<AnalogValue?> IDataProvider.GetAnalogValue(int serverId, ushort address, CancellationToken ct)
//    {
//        using var scope = _serviceProvider.CreateAsyncScope();
//        using var dbContext = scope.ServiceProvider.GetRequiredService<PowerUnitDbContext>();
//        return await _measurement(dbContext, serverId, address);
//    }

//    Task<DiscretValue?> IDataProvider.GetDiscretValue(int serverId, ushort address, CancellationToken ct)
//    {
//        return Task.FromResult<DiscretValue?>(default);
//    }
//}

