using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    internal void Process_C_CS_NA_1(in ASDUPacketHeader_2_2 header, ushort address, DateTime dateTime, TimeStatus timeStatus, CancellationToken ct)
    {
        SendInRentBuffer(static (buffer, context, additionInfo) =>
            {
                var headerReq = new ASDUPacketHeader_2_2(additionInfo.AsduType, additionInfo.SQ, additionInfo.Count,
                    COT.ACTIVATE_CONFIRMATION,
                    initAddr: additionInfo.InitAddr,
                    commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                var C_CS_NA_1 = new C_CS_NA_1(context._timeProvider.GetUtcNow().DateTime, TimeStatus.OK);
                var length = C_CS_NA_1.Serialize(buffer, in headerReq, in C_CS_NA_1);
                context._packetSender!.Send(buffer.AsSpan(0, length));
            }, this, header);
    }
}

