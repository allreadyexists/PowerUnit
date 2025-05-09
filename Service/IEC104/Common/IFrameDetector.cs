namespace PowerUnit;

public interface IFrameDetector
{
    void AssignNotified(INotifyPacket notified);
    void TryGetFrame(byte[] data);
    void Reset();
}
