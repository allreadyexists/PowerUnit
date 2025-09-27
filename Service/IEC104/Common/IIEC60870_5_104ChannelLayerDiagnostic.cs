namespace PowerUnit.Service.IEC104.Abstract;

public interface IIEC60870_5_104ChannelLayerDiagnostic
{
    void AppMsgTotal(string serverId, ChannelLayerPacketPriority priority);
    void AppMsgSkip(string serverId, ChannelLayerPacketPriority priority);
    void AppMsgSend(string serverId, ChannelLayerPacketPriority priority);

    void SendIPacket(string serverId);
    void SendUPacket(string serverId);
    void SendSPacket(string serverId);

    void RcvIPacket(string serverId);
    void RcvUPacket(string serverId);
    void RcvSPacket(string serverId);

    void ProtocolError(string serverId);
}

