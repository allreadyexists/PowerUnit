using FastEnumUtility;

using PowerUnit.Service.IEC104.Abstract;

using System.Diagnostics.Metrics;

namespace PowerUnit.Service.IEC104.Export;

public class IEC104Diagnostic : IIEC60870_5_104ChannelLayerDiagnostic, IIEC60870_5_104ApplicationLayerDiagnostic, IDisposable
{
    public static readonly string MeterName = nameof(IIEC60870_5_104ChannelLayerDiagnostic);
    private readonly Meter _meter;

    private readonly Counter<long> _appMsgSend;
    private readonly Counter<long> _appMsgSkip;
    private readonly Counter<long> _appMsgTotal;

    private readonly Counter<long> _sndPacket;
    private readonly Counter<long> _rcvPacket;

    private readonly Counter<long> _protocolError;

    private readonly Gauge<double> _sendMsgPrepareDuration;
    public IEC104Diagnostic(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(new MeterOptions(MeterName));

        _sndPacket = _meter.CreateCounter<long>("snd-packet");
        _rcvPacket = _meter.CreateCounter<long>("rcv-packet");

        _protocolError = _meter.CreateCounter<long>("protocol-error");

        _appMsgSend = _meter.CreateCounter<long>("app-msg-send");
        _appMsgSkip = _meter.CreateCounter<long>("app-msg-skip");
        _appMsgTotal = _meter.CreateCounter<long>("app-msg-total");

        _sendMsgPrepareDuration = _meter.CreateGauge<double>("app-send-msg-prepare-duration", "ns");
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgSend(int serverId, ChannelLayerPacketPriority priority)
    {
        _appMsgSend.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>(nameof(priority), priority.FastToString())
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgSkip(int serverId, ChannelLayerPacketPriority priority)
    {
        _appMsgSkip.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>(nameof(priority), priority.FastToString())
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgTotal(int serverId, ChannelLayerPacketPriority priority)
    {
        _appMsgTotal.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>(nameof(priority), priority.FastToString())
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendIPacket(int serverId)
    {
        _sndPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>("type", "I"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendUPacket(int serverId)
    {
        _sndPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>("type", "U"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendSPacket(int serverId)
    {
        _sndPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>("type", "S"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvIPacket(int serverId)
    {
        _rcvPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>("type", "I"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvUPacket(int serverId)
    {
        _rcvPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>("type", "U"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvSPacket(int serverId)
    {
        _rcvPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()),
            KeyValuePair.Create<string, object?>("type", "S"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.ProtocolError(int serverId)
    {
        _protocolError.Add(1, KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()));
    }

    void IIEC60870_5_104ApplicationLayerDiagnostic.AppSendMsgPrepareDuration(int serverId, double duration)
    {
        _sendMsgPrepareDuration.Record(duration, KeyValuePair.Create<string, object?>(nameof(serverId), serverId.ToString()));
    }

    void IDisposable.Dispose() => _meter.Dispose();
}

