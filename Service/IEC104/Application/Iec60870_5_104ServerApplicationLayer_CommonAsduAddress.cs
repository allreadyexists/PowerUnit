using PowerUnit.Common.StructHelpers;
using PowerUnit.Service.IEC104.Types;

using System.Collections.Immutable;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    private readonly HashSet<ASDUType> _broadcastEnables = [ASDUType.C_IC_NA_1, ASDUType.C_CI_NA_1, ASDUType.C_CS_NA_1, ASDUType.C_RP_NA_1];

    private struct AdditionInfo_AsduInfoRawArray
    {
        public ASDUPacketHeader_2_2 Header;
        public ImmutableArray<byte> AsduInfoRawArray;
        public COT COT;
    }

    private void SendOnError(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, COT cot)
    {
        SendInRentBuffer(static (buffer, context, additionInfo) =>
        {
            var headerReq = new ASDUPacketHeader_2_2(additionInfo.Header.AsduType, additionInfo.Header.SQ, additionInfo.Header.Count,
                additionInfo.COT,
                pn: PN.Negative,
                tn: additionInfo.Header.TN,
                initAddr: additionInfo.Header.InitAddr,
                commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
            headerReq.SerializeUnsafe(buffer, 0);
            additionInfo.AsduInfoRawArray.CopyTo(0, buffer, ASDUPacketHeader_2_2.Size, additionInfo.AsduInfoRawArray.Length);
            context._packetSender!.Send(buffer.AsSpan(0, ASDUPacketHeader_2_2.Size + additionInfo.AsduInfoRawArray.Length));
        }, this, new AdditionInfo_AsduInfoRawArray() { Header = header, AsduInfoRawArray = asduInfoRaw.ToImmutableArray(), COT = cot });
    }

    internal bool Process_Notify_CommonAsduAddress(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, CancellationToken ct)
    {
        if (header.CommonAddrAsdu == 0xFFFF)
        {
            // Широковещательные запросы определенных типов выполняются без проверок
            if (_broadcastEnables.Contains(header.AsduType))
                return true;
            // Остальные отбиваем
            else
            {
                SendOnError(in header, asduInfoRaw, COT.UNKNOWN_COMMON_ASDU_ADDRESS);
                return false;
            }
        }

        return !_applicationLayerOption.CheckCommonASDUAddress || _applicationLayerOption.CommonASDUAddress == header.CommonAddrAsdu;
    }
}

