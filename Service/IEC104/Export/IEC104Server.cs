using NetCoreServer;

using PowerUnit.Service.IEC104.Options.Models;

using System.Net;

namespace PowerUnit.Service.IEC104.Export;

public sealed class IEC104Server : TcpServer
{
    private readonly IServiceProvider _provider;
    private readonly IEC104ServerModel _options;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly ILogger<IEC104Server> _logger;
#pragma warning restore IDE0052 // Remove unread private members

    public IEC104Server(IServiceProvider provider, IEC104ServerModel options, ILogger<IEC104Server> logger) : base(IPAddress.Any, options.Port)
    {
        _provider = provider;
        _options = options;
        _logger = logger;
    }

    protected sealed override TcpSession CreateSession()
    {
        return new IEC104ServerSession(_provider, _options, this);
    }
}
