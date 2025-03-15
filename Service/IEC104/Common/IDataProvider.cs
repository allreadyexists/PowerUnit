namespace PowerUnit.Service.IEC104.Abstract;

public interface IDataProvider
{
    IEnumerable<MapValueItem> GetGroup(byte group);

    MapValueItem? GetValue(ushort address);
}
