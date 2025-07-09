namespace PowerUnit.Service.IEC104.Abstract;

public interface IFrameDetector
{
    void AssignNotified(INotifyPacket notified);
    void TryGetFrame(byte[] data);
    void Reset();
}
