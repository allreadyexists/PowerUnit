using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    internal void Process_Notify_Unknown_Asdu_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, CancellationToken ct)
    {
        SendOnError(in header, asduInfoRaw, COT.UNKNOWN_TYPE_ID);
    }
}

