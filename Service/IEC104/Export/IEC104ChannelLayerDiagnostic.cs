using PowerUnit.Service.IEC104.Abstract;

using System.Diagnostics.Metrics;

namespace PowerUnit.Service.IEC104.Channel;

public class IEC104ChannelLayerDiagnostic : IIEC60870_5_104ChannelLayerDiagnostic, IDisposable
{
    public static readonly string MeterName = nameof(IIEC60870_5_104ChannelLayerDiagnostic);
    private readonly Meter _meter;

    private readonly Counter<long> _sendMsg;
    private readonly Counter<long> _sendMgsAddToQueue;
    private readonly Counter<long> _sendMgsSkip;
    private readonly Counter<long> _sendMgsTake;

    private readonly Counter<long> _rcvIPacket;
    private readonly Counter<long> _rcvUPacket;
    private readonly Counter<long> _rcvSPacket;

    private readonly Counter<long> _protocolError;

    public IEC104ChannelLayerDiagnostic(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(new MeterOptions(MeterName));

        _sendMsg = _meter.CreateCounter<long>("send-msg");
        _sendMgsAddToQueue = _meter.CreateCounter<long>("send-msg-add-to-queue");
        _sendMgsSkip = _meter.CreateCounter<long>("send-msg-skip");
        _sendMgsTake = _meter.CreateCounter<long>("send-msg-take");

        _rcvIPacket = _meter.CreateCounter<long>("rcv-i-packet");
        _rcvUPacket = _meter.CreateCounter<long>("rcv-u-packet");
        _rcvSPacket = _meter.CreateCounter<long>("rcv-s-packet");

        _protocolError = _meter.CreateCounter<long>("protocol-error");
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendMgs(int serverId)
    {
        _sendMsg.Add(1, KeyValuePair.Create<string, object?>(nameof(serverId), serverId));
    }
    void IIEC60870_5_104ChannelLayerDiagnostic.SendMgsAddToQueue(int serverId, ChannelLayerPacketPriority priority)
    {
        _sendMgsAddToQueue.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>(nameof(priority), priority)
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendMgsSkip(int serverId, ChannelLayerPacketPriority priority)
    {
        _sendMgsSkip.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>(nameof(priority), priority)
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendMgsTake(int serverId, ChannelLayerPacketPriority priority)
    {
        _sendMgsTake.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>(nameof(priority), priority)
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvIPacket(int serverId)
    {
        _rcvIPacket.Add(1, KeyValuePair.Create<string, object?>(nameof(serverId), serverId));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvUPacket(int serverId)
    {
        _rcvUPacket.Add(1, KeyValuePair.Create<string, object?>(nameof(serverId), serverId));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvSPacket(int serverId)
    {
        _rcvSPacket.Add(1, KeyValuePair.Create<string, object?>(nameof(serverId), serverId));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.ProtocolError(int serverId)
    {
        _protocolError.Add(1, KeyValuePair.Create<string, object?>(nameof(serverId), serverId));
    }

    void IDisposable.Dispose() => _meter.Dispose();
}

