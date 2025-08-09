namespace PowerUnit.Service.IEC104.Abstract;

public interface IIEC60870_5_104ChannelLayerDiagnostic
{
    void AppMsgTotal(int serverId, ChannelLayerPacketPriority priority);
    void AppMsgSkip(int serverId, ChannelLayerPacketPriority priority);
    void AppMsgSend(int serverId, ChannelLayerPacketPriority priority);

    void SendIPacket(int serverId);
    void SendUPacket(int serverId);
    void SendSPacket(int serverId);

    void RcvIPacket(int serverId);
    void RcvUPacket(int serverId);
    void RcvSPacket(int serverId);

    void ProtocolError(int serverId);
}

