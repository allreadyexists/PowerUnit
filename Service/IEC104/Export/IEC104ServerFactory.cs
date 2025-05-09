using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PowerUnit;

public class IEC104ServerFactory
{
    private readonly IServiceProvider _provider;

    public IEC104ServerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IEC104Server CreateServer(IEC104ServerModel options)
    {
        return new IEC104Server(_provider, options, _provider.GetRequiredService<ILogger<IEC104Server>>());
    }
}
