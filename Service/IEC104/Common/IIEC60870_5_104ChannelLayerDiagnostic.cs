namespace PowerUnit.Service.IEC104.Abstract;

public interface IIEC60870_5_104ChannelLayerDiagnostic
{
    void SendMgsTake(int serverId, ChannelLayerPacketPriority priority);
    void SendMgsSkip(int serverId, ChannelLayerPacketPriority priority);
    void SendMgsAddToQueue(int serverId, ChannelLayerPacketPriority priority);
    void SendMgs(int serverId);

    void RcvIPacket(int serverId);
    void RcvUPacket(int serverId);
    void RcvSPacket(int serverId);

    void ProtocolError(int serverId);
}

