using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    internal void Process_C_CI_NA_1(AsduPacketHeader_2_2 header, ushort address, QCC qcc, CancellationToken ct)
    {
        _ = SendInRentBuffer(buffer =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                    COT.UNKNOWN_TYPE_ID,
                    pn: PN.Negative,
                    initAddr: header.InitAddr,
                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_CI_NA_1 = new C_CI_NA_1(qcc);
                var length = C_CI_NA_1.Serialize(buffer, in headerReq, in C_CI_NA_1);
                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
    }
}

