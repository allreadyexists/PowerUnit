using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    private struct AdditionInfo_C_TS_NA_1
    {
        public ASDUPacketHeader_2_2 Header;
        public ushort Address;
        public ushort FBP;
    }

    internal void Process_C_TS_NA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort fbp, CancellationToken ct)
    {
        SendInRentBuffer(static (buffer, context, additionInfo) =>
            {
                var headerReq = new ASDUPacketHeader_2_2(additionInfo.Header.AsduType, additionInfo.Header.SQ, additionInfo.Header.Count,
                COT.ACTIVATE_CONFIRMATION,
                PN.Positive,
                initAddr: additionInfo.Header.InitAddr,
                commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                var C_TS_NA_1 = new C_TS_NA_1(additionInfo.FBP);
                var length = C_TS_NA_1.Serialize(buffer, in headerReq, in C_TS_NA_1);
                context._packetSender!.Send(buffer.AsSpan(0, length));
            }, this, new AdditionInfo_C_TS_NA_1() { Header = header, Address = address, FBP = fbp });
    }

    private struct AdditionInfo_C_TS_TA_1
    {
        public ASDUPacketHeader_2_2 Header;
        public ushort Address;
        public ushort TSC;
        public DateTime DateTime;
        public TimeStatus Status;
    }

    internal void Process_C_TS_TA_1(in ASDUPacketHeader_2_2 header, ushort address, ushort tsc, DateTime dateTime, TimeStatus status, CancellationToken ct)
    {
        SendInRentBuffer(static (buffer, context, additionInfo) =>
            {
                var headerReq = new ASDUPacketHeader_2_2(additionInfo.Header.AsduType, additionInfo.Header.SQ, additionInfo.Header.Count,
                COT.ACTIVATE_CONFIRMATION,
                PN.Positive,
                initAddr: additionInfo.Header.InitAddr,
                commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                var C_TS_TA_1 = new C_TS_TA_1(additionInfo.TSC, additionInfo.DateTime, additionInfo.Status);
                var length = C_TS_TA_1.Serialize(buffer, in headerReq, in C_TS_TA_1);
                context._packetSender!.Send(buffer.AsSpan(0, length));
            }, this, new AdditionInfo_C_TS_TA_1() { Header = header, Address = address, TSC = tsc, DateTime = dateTime, Status = status });
    }
}

