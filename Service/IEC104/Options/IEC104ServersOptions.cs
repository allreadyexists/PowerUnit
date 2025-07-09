namespace PowerUnit.Service.IEC104.Options;

public sealed record class IEC104ServersOptions
{
    public IEC104ServerOptions[] Servers { get; set; } = [];
}

