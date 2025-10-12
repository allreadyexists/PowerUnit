using Microsoft.Extensions.Logging;

using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Types;

using System.Collections.Frozen;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104ServerDataStreamSource : DataSourceBase<MapValueItem>
{
    private readonly Subscriber<BaseValue, IEC104ServerDataStreamSource>? _subscriber;

    private readonly FrozenDictionary<(string SourceId, string EquipmentId, string ParameterId), IEC104MappingModel> _mapping;

    private static Task Subscribe(BaseValue value, IEC104ServerDataStreamSource context, CancellationToken ct)
    {
        if (context._mapping.TryGetValue((value.SourceId, value.EquipmentId, value.ParameterId), out var map))
        {
            var newValue = new MapValueItem(map.Address, (ASDUType)map.AsduType)
            {
                Value = value
            };
            context.Notify(ref newValue);
        }

        return Task.CompletedTask;
    }

    public IEC104ServerDataStreamSource(IDataSource<BaseValue> source, bool sporadicSendEnable, FrozenDictionary<(string SourceId, string EquipmentId, string ParameterId), IEC104MappingModel> mapping, ISubscriberDiagnostic diagnostic, ILogger<IEC104ServerDataStreamSource> logger) : base(logger)
    {
        _mapping = mapping;
        if (sporadicSendEnable && _mapping.Count != 0)
        {
            _subscriber = new SubscriberBounded<BaseValue, IEC104ServerDataStreamSource>(1, source,
                this, Subscribe, subscriberDiagnostic: diagnostic);
        }
    }

    public override void Dispose()
    {
        (_subscriber as IDisposable)?.Dispose();
    }
}
