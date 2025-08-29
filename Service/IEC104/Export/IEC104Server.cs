using NetCoreServer;

using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Models;

using System.Collections.Frozen;
using System.Net;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104Server : TcpServer
{
    private readonly IServiceProvider _provider;
    private readonly IEC104ServerModel _options;

    private readonly FrozenDictionary<(string SourceId, string EquipmentId, string ParameterId), IEC104MappingModel> _mapping;
    private readonly FrozenDictionary<byte, FrozenSet<ushort>> _groups;

    private readonly IDataSource<MapValueItem> _dataSource;
    private readonly IDataProvider _dataProvider;

    public IEC104Server(IServiceProvider provider, IEC104ServerModel options, IEnumerable<IEC104MappingModel> mapping) : base(IPAddress.Any, options.Port)
    {
        _provider = provider;
        _options = options;

        var dictionary = new Dictionary<(string, string, string), IEC104MappingModel>();
        foreach (var map in mapping.GroupBy(x => (x.SourceId, x.EquipmentId, x.ParameterId, x.Address, x.AsduType)))
        {
            dictionary[(map.Key.SourceId, map.Key.EquipmentId, map.Key.ParameterId)] = map.First();
        }

        _mapping = dictionary.ToFrozenDictionary();
        _groups = mapping.GroupBy(x => x.Group).ToFrozenDictionary(x => x.Key, y => y.Select(v => v.Address).ToFrozenSet());

        _dataProvider = ActivatorUtilities.CreateInstance<IEC104DataProvider>(_provider, [_mapping, _groups]);
        _dataSource = ActivatorUtilities.CreateInstance<IEC104ServerDataStreamSource>(_provider, [options.ApplicationLayerModel.SporadicSendEnabled, _mapping]);
    }

    protected sealed override TcpSession CreateSession()
    {
        return ActivatorUtilities.CreateInstance<IEC104ServerSession>(_provider, [_options, _dataSource, _dataProvider, this]);
    }

    protected sealed override void Dispose(bool disposingManagedResources)
    {
        base.Dispose(disposingManagedResources);
        (_dataSource as IDisposable)?.Dispose();
        (_dataProvider as IDisposable)?.Dispose();
    }
}
