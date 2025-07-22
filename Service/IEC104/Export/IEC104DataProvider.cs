
using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;

using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104DataProvider : IDataProvider, IDisposable
{
    private static readonly int _bufferizationSize = 100;
    private static readonly TimeSpan _bufferizationTimeout = TimeSpan.FromMilliseconds(500);

    private readonly FrozenDictionary<(long EquipmentId, long ParameterId), IEC104MappingModel> _mapping;
    private readonly FrozenDictionary<byte, FrozenSet<ushort>> _groups;
    private readonly SubscriberBase<BaseValue>? _subscriber;

    private readonly ConcurrentDictionary<ushort, MapValueItem> _values = new ConcurrentDictionary<ushort, MapValueItem>();

    public IEC104DataProvider(IDataSource<BaseValue> source,
        FrozenDictionary<(long EquipmentId, long ParameterId), IEC104MappingModel> mapping,
        FrozenDictionary<byte, FrozenSet<ushort>> groups,
        ILogger<IEC104DataProvider> logger)
    {
        _mapping = mapping;
        _groups = groups;
        if (_mapping.Count != 0)
        {
            _subscriber = new BatchSubscriber<BaseValue>(_bufferizationSize, _bufferizationTimeout, source, values =>
            {
                Snapshot(values);
                return Task.CompletedTask;
            }, filter: ValueFilter);
        }
    }

    void IDisposable.Dispose() => (_subscriber as IDisposable)?.Dispose();
    IEnumerable<MapValueItem> IDataProvider.GetGroup(byte group)
    {
        if (_groups.TryGetValue(group, out var g))
            return _values.Where(x => g.Contains(x.Key)).Select(x => new MapValueItem(x.Key, x.Value.Type, x.Value.Value));
        return Array.Empty<MapValueItem>();
    }
    MapValueItem? IDataProvider.GetValue(ushort address)
    {
        if (_values.TryGetValue(address, out var value))
            return value;
        return null;
    }

    private void Snapshot(IEnumerable<BaseValue> values)
    {
        foreach (var value in values)
        {
            if (_mapping.TryGetValue((value.EquipmentId, value.ParameterId), out var v))
                _values.AddOrUpdate(v.Address,
                    address => new MapValueItem(v.Address, (ASDUType)v.AsduType, value),
                    (address, oldValue) => new MapValueItem(v.Address, (ASDUType)v.AsduType, value));
        }
    }

    private bool ValueFilter(BaseValue value)
    {
        return _mapping.ContainsKey((value.EquipmentId, value.ParameterId));
    }
}
