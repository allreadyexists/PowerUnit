
using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Types;

using System.Collections.Frozen;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104ServerDataStreamSource : DataSourceBase<MapValueItem>
{
    private readonly Subscriber<BaseValue>? _subscriber;

    private readonly FrozenDictionary<(long EquipmentId, long ParameterId), IEC104MappingModel> _mapping;

    public IEC104ServerDataStreamSource(IDataSource<BaseValue> source, bool sporadicSendEnable, FrozenDictionary<(long EquipmentId, long ParameterId), IEC104MappingModel> mapping, ILogger<IEC104ServerDataStreamSource> logger) : base(logger)
    {
        _mapping = mapping;
        if (sporadicSendEnable && _mapping.Count != 0)
        {
            _subscriber = new SubscriberBounded<BaseValue>(1, source,
                x =>
                {
                    var map = _mapping[(x.EquipmentId, x.ParameterId)];
                    Notify(new MapValueItem(map.Address, (ASDUType)map.AsduType, x));
                    return Task.CompletedTask;
                }, filter: ValueFilter);
        }
    }

    private bool ValueFilter(BaseValue value) => _mapping.ContainsKey((value.EquipmentId, value.ParameterId));

    public override void Dispose()
    {
        (_subscriber as IDisposable)?.Dispose();
    }
}
