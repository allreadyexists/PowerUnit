using PowerUnit.Asdu;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    internal void Process_C_TS_NA_1(AsduPacketHeader_2_2 header, ushort address, ushort fbp, CancellationToken ct)
    {
        _ = SendInRentBuffer(buffer =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                COT.ACTIVATE_CONFIRMATION,
                PN.Positive,
                initAddr: header.InitAddr,
                commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_TS_NA_1 = new C_TS_NA_1(fbp);
                var length = C_TS_NA_1.Serialize(buffer, ref headerReq, ref C_TS_NA_1);
                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
    }

    internal void Process_C_TS_TA_1(AsduPacketHeader_2_2 header, ushort address, ushort tsc, DateTime dateTime, TimeStatus status, CancellationToken ct)
    {
        _ = SendInRentBuffer(buffer =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                COT.ACTIVATE_CONFIRMATION,
                PN.Positive,
                initAddr: header.InitAddr,
                commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_TS_TA_1 = new C_TS_TA_1(tsc, dateTime, status);
                var length = C_TS_TA_1.Serialize(buffer, ref headerReq, ref C_TS_TA_1);
                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
    }
}

