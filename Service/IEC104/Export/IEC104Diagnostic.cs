using FastEnumUtility;

using PowerUnit.Common.Subsciption;
using PowerUnit.Common.TimeoutService;
using PowerUnit.DataSource.Test;
using PowerUnit.Service.IEC104.Abstract;

using System.Diagnostics.Metrics;

namespace PowerUnit.Service.IEC104.Export;

public class IEC104Diagnostic :
    IIEC60870_5_104ChannelLayerDiagnostic,
    IIEC60870_5_104ApplicationLayerDiagnostic,
    ITimeoutServiceDiagnostic,
    ITestDataSourceDiagnostic,
    ISubscriberDiagnostic,
    IDisposable
{
    public static readonly string MeterName = nameof(IEC104Diagnostic);
    private readonly Meter _meter;

    private readonly Counter<long> _appMsgSend;
    private readonly Counter<long> _appMsgSkip;
    private readonly Counter<long> _appMsgTotal;

    private readonly Counter<long> _sndPacket;
    private readonly Counter<long> _rcvPacket;

    private readonly Counter<long> _protocolError;

    private readonly Gauge<double> _sendMsgPrepareDuration;

    private readonly Counter<long> _timerOperationCall;
    private readonly Gauge<double> _timerOperationDuration;

    private readonly Counter<long> _testMsgCount;

    private readonly Counter<long> _subscriberRcv;
    private readonly Counter<long> _subscriberProcess;
    private readonly Counter<long> _subscriberDrop;

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

        _timerOperationCall = _meter.CreateCounter<long>("timer-operation-call");
        _timerOperationDuration = _meter.CreateGauge<double>("timer-operation-call-duration", "ns");

        _testMsgCount = _meter.CreateCounter<long>("test-msg-total");

        _subscriberRcv = _meter.CreateCounter<long>("subscriber-msg-rcv");
        _subscriberProcess = _meter.CreateCounter<long>("subscriber-msg-process");
        _subscriberDrop = _meter.CreateCounter<long>("subscriber-msg-drop");
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgSend(string serverId, ChannelLayerPacketPriority priority)
    {
        _appMsgSend.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>(nameof(priority), priority.FastToString())
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgSkip(string serverId, ChannelLayerPacketPriority priority)
    {
        _appMsgSkip.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>(nameof(priority), priority.FastToString())
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.AppMsgTotal(string serverId, ChannelLayerPacketPriority priority)
    {
        _appMsgTotal.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>(nameof(priority), priority.FastToString())
            );
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendIPacket(string serverId)
    {
        _sndPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>("type", "I"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendUPacket(string serverId)
    {
        _sndPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>("type", "U"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.SendSPacket(string serverId)
    {
        _sndPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>("type", "S"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvIPacket(string serverId)
    {
        _rcvPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>("type", "I"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvUPacket(string serverId)
    {
        _rcvPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>("type", "U"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.RcvSPacket(string serverId)
    {
        _rcvPacket.Add(1,
            KeyValuePair.Create<string, object?>(nameof(serverId), serverId),
            KeyValuePair.Create<string, object?>("type", "S"));
    }

    void IIEC60870_5_104ChannelLayerDiagnostic.ProtocolError(string serverId)
    {
        _protocolError.Add(1, KeyValuePair.Create<string, object?>(nameof(serverId), serverId));
    }

    void IIEC60870_5_104ApplicationLayerDiagnostic.AppSendMsgPrepareDuration(string serverId, double duration)
    {
        _sendMsgPrepareDuration.Record(duration, KeyValuePair.Create<string, object?>(nameof(serverId), serverId));
    }

    void IDisposable.Dispose() => _meter.Dispose();
    void ITimeoutServiceDiagnostic.TimerCallbackDuration(double duration)
    {
        _timerOperationDuration.Record(duration, KeyValuePair.Create<string, object?>("type", "Callback"));
    }
    void ITimeoutServiceDiagnostic.TimerCallbackCall()
    {
        _timerOperationCall.Add(1, KeyValuePair.Create<string, object?>("type", "Callback"));
    }
    void ITimeoutServiceDiagnostic.CreateTimeoutDuration(double duration)
    {
        _timerOperationDuration.Record(duration, KeyValuePair.Create<string, object?>("type", "Create"));
    }
    void ITimeoutServiceDiagnostic.CreateTimeoutCall()
    {
        _timerOperationCall.Add(1, KeyValuePair.Create<string, object?>("type", "Create"));
    }
    void ITimeoutServiceDiagnostic.RestartTimeoutDuration(double duration)
    {
        _timerOperationDuration.Record(duration, KeyValuePair.Create<string, object?>("type", "Restart"));
    }
    void ITimeoutServiceDiagnostic.RestartTimeoutCall()
    {
        _timerOperationCall.Add(1, KeyValuePair.Create<string, object?>("type", "Restart"));
    }
    void ITimeoutServiceDiagnostic.CancelTimeoutDuration(double duration)
    {
        _timerOperationDuration.Record(duration, KeyValuePair.Create<string, object?>("type", "Cancel"));
    }
    void ITimeoutServiceDiagnostic.CancelTimeoutCall()
    {
        _timerOperationCall.Add(1, KeyValuePair.Create<string, object?>("type", "Cancel"));
    }

    void ITestDataSourceDiagnostic.IncRequest()
    {
        _testMsgCount.Add(1);
    }

    void ISubscriberDiagnostic.RcvCounter(string source)
    {
        _subscriberRcv.Add(1, KeyValuePair.Create<string, object?>("subscriber", source));
    }

    void ISubscriberDiagnostic.ProcessCounter(string source)
    {
        _subscriberProcess.Add(1, KeyValuePair.Create<string, object?>("subscriber", source));
    }

    void ISubscriberDiagnostic.DropCounter(string source)
    {
        _subscriberDrop.Add(1, KeyValuePair.Create<string, object?>("subscriber", source));
    }
}

