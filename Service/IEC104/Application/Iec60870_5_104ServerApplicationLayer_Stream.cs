using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;

namespace PowerUnit;

public sealed partial class Iec60870_5_104ServerApplicationLayer
{
    private void Snapshot(IEnumerable<BaseValue> values)
    {
        foreach (var value in values)
        {
            if (_mapping.TryGetValue((value.EquipmentId, value.ParameterId), out var v))
                _values.AddOrUpdate((ushort)v.Address, address => (v.AsduType, value), (address, oldValue) => (v.AsduType, value));
        }
    }

    private void Stream(byte[] buffer, IEnumerable<BaseValue> values)
    {
        SendValues(buffer, 0, COT.SPORADIC, values.Select(x =>
        {
            var map = _mapping[(x.EquipmentId, x.ParameterId)];
            return KeyValuePair.Create((ushort)map.Address, (map.AsduType, x));
        }));
    }

    private bool ValueFilter(BaseValue value)
    {
        return _mapping.ContainsKey((value.EquipmentId, value.ParameterId));
    }
}

