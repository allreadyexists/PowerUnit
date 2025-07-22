namespace PowerUnit.Service.IEC104.Export;

public class IEC104ServerFactory
{
    private readonly IServiceProvider _provider;

    public IEC104ServerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IEC104Server CreateServer(IEC104ServerModel options, IEnumerable<IEC104MappingModel> mapping) =>
        ActivatorUtilities.CreateInstance<IEC104Server>(_provider, [options, mapping]);
}
