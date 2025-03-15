using PowerUnit.Common.StructHelpers;
using PowerUnit.Service.IEC104.Types;

using System.Collections.Immutable;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    internal void Process_Notify_Unknown_Cot_Raw(ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, CancellationToken ct)
    {
        var asduInfoRawArray = asduInfoRaw.ToImmutableArray();

        _ = SendInRentBuffer(buffer =>
            {
                var headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                    COT.UNKNOWN_TRANSFER_REASON,
                    pn: PN.Negative,
                    tn: header.TN,
                    initAddr: header.InitAddr,
                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                headerReq.SerializeUnsafe(buffer, 0);
                asduInfoRawArray.CopyTo(0, buffer, ASDUPacketHeader_2_2.Size, asduInfoRawArray.Length);
                _packetSender!.Send(buffer[..(ASDUPacketHeader_2_2.Size + asduInfoRawArray.Length)]);
                return Task.CompletedTask;
            });
    }
}

