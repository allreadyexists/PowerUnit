using NetCoreServer;

using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104.Abstract;

using System.Collections.Frozen;
using System.Net;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104Server : TcpServer
{
    private readonly IServiceProvider _provider;
    private readonly IEC104ServerModel _options;

    private readonly FrozenDictionary<(long EquipmentId, long ParameterId), IEC104MappingModel> _mapping;
    private readonly FrozenDictionary<byte, FrozenSet<ushort>> _groups;

    private readonly IEC104ServerDataSource _dataSource;
    private readonly IDataProvider _dataProvider;

#pragma warning disable IDE0052 // Remove unread private members
    private readonly ILogger<IEC104Server> _logger;
#pragma warning restore IDE0052 // Remove unread private members

    public IEC104Server(IServiceProvider provider, IEC104ServerModel options, IEnumerable<IEC104MappingModel> mapping, ILogger<IEC104Server> logger) : base(IPAddress.Any, options.Port)
    {
        _provider = provider;
        _options = options;

        var dictionary = new Dictionary<(long, long), IEC104MappingModel>();
        foreach (var map in mapping.GroupBy(x => (x.EquipmentId, x.ParameterId, x.Address, x.AsduType)))
        {
            dictionary[(map.Key.EquipmentId, map.Key.ParameterId)] = map.First();
        }

        _mapping = dictionary.ToFrozenDictionary();
        _groups = mapping.GroupBy(x => x.Group).ToFrozenDictionary(x => x.Key, y => y.Select(v => v.Address).ToFrozenSet());

        _logger = logger;
        _dataProvider = new IEC104DataProvider(_provider.GetRequiredService<IDataSource<BaseValue>>(), _mapping, _groups,
            _provider.GetRequiredService<ILogger<IEC104ServerDataSource>>());
        _dataSource = new IEC104ServerDataSource(_provider.GetRequiredService<IDataSource<BaseValue>>(),
            _options.ApplicationLayerModel.SporadicSendEnabled,
            _mapping, _provider.GetRequiredService<ILogger<IEC104ServerDataSource>>());
    }

    protected sealed override TcpSession CreateSession()
    {
        return new IEC104ServerSession(_provider, _options, _dataSource, _dataProvider, this);
    }

    protected sealed override void Dispose(bool disposingManagedResources)
    {
        base.Dispose(disposingManagedResources);
        _dataSource.Dispose();
        (_dataProvider as IDisposable)?.Dispose();
    }
}
