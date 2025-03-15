namespace PowerUnit.Service.IEC104.Abstract;

public interface IPhysicalLayerCommander
{
    long SendPacket(byte[] buffer, long offset, long size);
    bool DisconnectLayer();
}
