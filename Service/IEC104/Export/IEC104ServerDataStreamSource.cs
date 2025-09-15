using Microsoft.Extensions.Logging;

using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Types;

using System.Collections.Frozen;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104ServerDataStreamSource : DataSourceBase<MapValueItem>
{
    private readonly Subscriber<BaseValue, IEC104ServerDataStreamSource>? _subscriber;

    private readonly FrozenDictionary<(string SourceId, string EquipmentId, string ParameterId), IEC104MappingModel> _mapping;

    public IEC104ServerDataStreamSource(IDataSource<BaseValue> source, bool sporadicSendEnable, FrozenDictionary<(string SourceId, string EquipmentId, string ParameterId), IEC104MappingModel> mapping, ILogger<IEC104ServerDataStreamSource> logger) : base(logger)
    {
        _mapping = mapping;
        if (sporadicSendEnable && _mapping.Count != 0)
        {
            _subscriber = new SubscriberBounded<BaseValue, IEC104ServerDataStreamSource>(1, source, this,
                static (x, context, token) =>
                {
                    var map = context._mapping[(x.SourceId, x.EquipmentId, x.ParameterId)];
                    context.Notify(new MapValueItem(map.Address, (ASDUType)map.AsduType)
                    {
                        Value = x
                    });
                    return Task.CompletedTask;
                }, filter: ValueFilter);
        }
    }

    private static bool ValueFilter(BaseValue value, IEC104ServerDataStreamSource context) => context._mapping.ContainsKey((value.SourceId, value.EquipmentId, value.ParameterId));

    public override void Dispose()
    {
        (_subscriber as IDisposable)?.Dispose();
    }
}
