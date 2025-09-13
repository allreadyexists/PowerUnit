using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    private struct AdditionInfo_C_CI_NA_1
    {
        public ASDUPacketHeader_2_2 Header { get; set; }

        public ushort Address { get; set; }
        public QCC QCC { get; set; }
    }

    internal void Process_C_CI_NA_1(in ASDUPacketHeader_2_2 header, ushort address, QCC qcc, CancellationToken ct)
    {
        SendInRentBuffer(static (buffer, context, additionInfo) =>
            {
                var headerReq = new ASDUPacketHeader_2_2(additionInfo.Header.AsduType, additionInfo.Header.SQ, additionInfo.Header.Count,
                    COT.UNKNOWN_TYPE_ID,
                    pn: PN.Negative,
                    initAddr: additionInfo.Header.InitAddr,
                    commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                var C_CI_NA_1 = new C_CI_NA_1(additionInfo.QCC);
                var length = C_CI_NA_1.Serialize(buffer, in headerReq, in C_CI_NA_1);
                context._packetSender!.Send(buffer.AsSpan(0, length));
            }, this, new AdditionInfo_C_CI_NA_1() { Header = header, Address = address, QCC = qcc });
    }
}

