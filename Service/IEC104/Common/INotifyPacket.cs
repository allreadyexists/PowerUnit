namespace PowerUnit.Service.IEC104.Abstract;

public interface INotifyPacket
{
    void NotifyPacketDetected(byte[] packet);
}

