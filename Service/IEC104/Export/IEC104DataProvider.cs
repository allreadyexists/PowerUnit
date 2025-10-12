using Microsoft.Extensions.Logging;

using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;

using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104DataProvider : IDataProvider, IDisposable
{
    private static readonly int _bufferizationSize = 512;
    private static readonly TimeSpan _bufferizationTimeout = TimeSpan.FromMilliseconds(500);

    private readonly FrozenDictionary<(string SourceId, string EquipmentId, string ParameterId), IEC104MappingModel> _mapping;
    private readonly FrozenDictionary<byte, FrozenSet<ushort>> _groups;
    private readonly SubscriberBase<BaseValue, IEC104DataProvider>? _subscriber;

    private readonly ReaderWriterLock _lock = new ReaderWriterLock();
    private readonly Dictionary<ushort, MapValueItem> _values = new Dictionary<ushort, MapValueItem>();

    public IEC104DataProvider(IDataSource<BaseValue> source,
        FrozenDictionary<(string SourceId, string EquipmentId, string ParameterId), IEC104MappingModel> mapping,
        FrozenDictionary<byte, FrozenSet<ushort>> groups,
        ISubscriberDiagnostic subscriberDiagnostic,
        ILogger<IEC104DataProvider> logger)
    {
        _mapping = mapping;
        _groups = groups;
        if (_mapping.Count != 0)
        {
            _subscriber = new BatchSubscriber<BaseValue, IEC104DataProvider>(_bufferizationSize, _bufferizationTimeout, source, this, static (values, context, token) =>
            {
                context.Snapshot(values);
                return Task.CompletedTask;
            }, filter: ValueFilter, subscriberDiagnostic: subscriberDiagnostic);
        }
    }

    void IDisposable.Dispose() => (_subscriber as IDisposable)?.Dispose();
    IEnumerable<MapValueItem> IDataProvider.GetGroup(byte group)
    {
        if (_groups.TryGetValue(group, out var g))
        {
            _lock.AcquireReaderLock(int.MaxValue);
            try
            {
                return _values.Where(x => g.Contains(x.Key)).Select(x => x.Value);
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }
        return Array.Empty<MapValueItem>();
    }
    MapValueItem? IDataProvider.GetValue(ushort address)
    {
        _lock.AcquireReaderLock(int.MaxValue);
        try
        {
            return _values.TryGetValue(address, out var value) ? value : null;
        }
        finally
        {
            _lock.ReleaseReaderLock();
        }
    }

    private void Snapshot(IList<BaseValue> values)
    {
        _lock.AcquireWriterLock(int.MaxValue);
        try
        {
            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                var v = _mapping[(value.SourceId, value.EquipmentId, value.ParameterId)];

                ref var valueItem = ref CollectionsMarshal.GetValueRefOrAddDefault(_values, v.Address, out bool exists);

                if (exists)
                {
                    valueItem.Value = value;
                }
                else
                {
                    _values[v.Address] = valueItem = new MapValueItem(v.Address, (ASDUType)v.AsduType);
                }
            }
        }
        finally
        {
            _lock.ReleaseWriterLock();
        }
    }

    private static bool ValueFilter(BaseValue value, IEC104DataProvider context)
    {
        return context._mapping.ContainsKey((value.SourceId, value.EquipmentId, value.ParameterId));
    }
}
