using PowerUnit.Common.Options;

namespace PowerUnit.Service.IEC104.Options;

public sealed record class IEC104ServerOptions : ServerModel
{
    public int Id { get; set; }
    public IEC104ChannelLayerOption ChannelLayerOption { get; set; } = new IEC104ChannelLayerOption();
    public IEC104ApplicationLayerOption ApplicationLayerOption { get; set; } = new IEC104ApplicationLayerOption();
}

