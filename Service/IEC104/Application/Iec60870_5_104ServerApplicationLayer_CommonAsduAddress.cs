using PowerUnit.Common.StructHelpers;
using PowerUnit.Service.IEC104.Types;

using System.Collections.Immutable;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    private readonly HashSet<ASDUType> _broadcastEnables = [ASDUType.C_IC_NA_1, ASDUType.C_CI_NA_1, ASDUType.C_CS_NA_1, ASDUType.C_RP_NA_1];

    internal bool Process_Notify_CommonAsduAddress(ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, CancellationToken ct)
    {
        if (header.CommonAddrAsdu == 0xFFFF)
        {
            // Широковещательные запросы определенных типов выполняются без проверок
            if (_broadcastEnables.Contains(header.AsduType))
                return true;
            // Остальные отбиваем
            else
            {
                var asduInfoRawArray = asduInfoRaw.ToImmutableArray();

                _ = SendInRentBuffer(buffer =>
                    {
                        var headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.UNKNOWN_COMMON_ASDU_ADDRESS, pn: PN.Negative, tn: header.TN, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                        headerReq.SerializeUnsafe(buffer, 0);
                        asduInfoRawArray.CopyTo(0, buffer, ASDUPacketHeader_2_2.Size, asduInfoRawArray.Length);
                        _packetSender!.Send(buffer[..(ASDUPacketHeader_2_2.Size + asduInfoRawArray.Length)]);
                        return Task.CompletedTask;
                    });

                return false;
            }
        }

        return !_applicationLayerOption.CheckCommonASDUAddress || _applicationLayerOption.CommonASDUAddress == header.CommonAddrAsdu;
    }
}

