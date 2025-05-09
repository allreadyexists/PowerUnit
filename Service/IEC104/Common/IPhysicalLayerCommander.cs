namespace PowerUnit;

public interface IPhysicalLayerCommander
{
    long SendPacket(byte[] buffer, long offset, long size);
    bool DisconnectLayer();
}
