using PowerUnit.Asdu;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    internal void Process_C_CS_NA_1(AsduPacketHeader_2_2 header, ushort address, DateTime dateTime, TimeStatus timeStatus, CancellationToken ct)
    {
        _ = SendInRentBuffer(buffer =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                    COT.ACTIVATE_CONFIRMATION,
                    initAddr: header.InitAddr,
                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_CS_NA_1 = new C_CS_NA_1(_timeProvider.GetUtcNow().DateTime, TimeStatus.OK);
                var length = C_CS_NA_1.Serialize(buffer, ref headerReq, ref C_CS_NA_1);
                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
    }
}

