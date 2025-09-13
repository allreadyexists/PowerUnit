namespace PowerUnit.Service.IEC104.Abstract;

public interface IIEC60870_5_104ApplicationLayerDiagnostic
{
    void AppSendMsgPrepareDuration(int serverId, double duration);
}

