using PowerUnit.Common.Options;

namespace PowerUnit.Service.IEC104.Options.Models;

public sealed record class IEC104ServerModel : ServerModel
{
    public IEC104ChannelLayerModel ChannelLayerModel { get; set; } = new IEC104ChannelLayerModel();
    public IEC104ApplicationLayerModel ApplicationLayerModel { get; set; } = new IEC104ApplicationLayerModel();
}

