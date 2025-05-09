using System.Collections.Immutable;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    private readonly HashSet<AsduType> _broadcastEnables = [AsduType.C_IC_NA_1, AsduType.C_CI_NA_1, AsduType.C_CS_NA_1, AsduType.C_RP_NA_1];

    internal bool Process_Notify_CommonAsduAddress(AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw, CancellationToken ct)
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
                        var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.UNKNOWN_COMMON_ASDU_ADDRESS, pn: PN.Negative, tn: header.TN, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                        headerReq.SerializeUnsafe(buffer, 0);
                        asduInfoRawArray.CopyTo(0, buffer, AsduPacketHeader_2_2.Size, asduInfoRawArray.Length);
                        _packetSender!.Send(buffer[..(AsduPacketHeader_2_2.Size + asduInfoRawArray.Length)]);
                        return Task.CompletedTask;
                    });

                return false;
            }
        }

        return !_applicationLayerOption.CheckCommonASDUAddress || _applicationLayerOption.CommonASDUAddress == header.CommonAddrAsdu;
    }
}

