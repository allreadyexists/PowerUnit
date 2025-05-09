using System.Collections.Immutable;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    internal void Process_Notify_Unknown_Exception(AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw, CancellationToken ct)
    {
        var asduInfoRawArray = asduInfoRaw.ToImmutableArray();

        _ = SendInRentBuffer(buffer =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                    COT.UNKNOWN_TRANSFER_REASON,
                    pn: PN.Negative,
                    tn: header.TN,
                    initAddr: header.InitAddr,
                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                headerReq.SerializeUnsafe(buffer, 0);
                asduInfoRawArray.CopyTo(0, buffer, AsduPacketHeader_2_2.Size, asduInfoRawArray.Length);
                _packetSender!.Send(buffer[..(AsduPacketHeader_2_2.Size + asduInfoRawArray.Length)]);
                return Task.CompletedTask;
            });
    }
}

