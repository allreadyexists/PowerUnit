namespace PowerUnit;

public sealed record class IEC104ServerOptions : ServerOptions
{
    public int Id { get; set; }
    public IEC104ChannelLayerOption ChannelLayerOption { get; set; } = new IEC104ChannelLayerOption();
    public IEC104ApplicationLayerOption ApplicationLayerOption { get; set; } = new IEC104ApplicationLayerOption();
}

