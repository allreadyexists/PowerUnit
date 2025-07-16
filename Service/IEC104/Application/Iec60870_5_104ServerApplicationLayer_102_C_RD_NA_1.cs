using Microsoft.Extensions.Logging;

using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    internal void Process_C_RD_NA_1(ASDUPacketHeader_2_2 header, ushort address, CancellationToken ct)
    {
        _ = SendInRentBuffer((buffer) =>
            {
                MapValueItem? result = null;
                var errorTransaction = false;
                var transactionId = (((int)header.AsduType) << 24) + (address << 8);

                try
                {
                    if (_readTransactionManager.CreateTransaction(transactionId, out var transactionCt))
                    {
                        result = _dataProvider.GetValue(address);
                    }
                    else
                    {
                        errorTransaction = true;
                        _logger.LogError($"Transaction duplicate: {header.AsduType} {address}");
                    }
                }
                catch (Exception ex)
                {
                    errorTransaction = true;
                    _logger.LogError(ex, $"Transaction error: {header.AsduType} {address}");
                }
                finally
                {
                    _readTransactionManager.DeleteTransaction(transactionId);
                }

                int length;

                if (result.HasValue)
                {
                    ASDUPacketHeader_2_2 headerReq;
                    switch (result.Value.Type)
                    {
                        case ASDUType.M_SP_TB_1 when result.Value.Value is DiscretValue discretValue1: // Одноэлементная информация с меткой времени СР56Время2а
                            headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var M_SP_TB_1 = new M_SP_TB_1_Single(address, discretValue1.Value ? SIQ_Value.On : SIQ_Value.Off, 0, result.Value.Value.ValueDt!.Value, TimeStatus.OK);
                            length = M_SP_TB_1_Single.Serialize(buffer, in headerReq, M_SP_TB_1);
                            break;
                        case ASDUType.M_DP_TB_1 when result.Value.Value is DiscretValue discretValue2: // Двухэлементная информация с меткой времени СР56Время2а
                            headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var M_DP_TB_1 = new M_DP_TB_1_Single(address, discretValue2.Value ? DIQ_Value.On : DIQ_Value.Off, 0, result.Value.Value.ValueDt!.Value, TimeStatus.OK);
                            length = M_DP_TB_1_Single.Serialize(buffer, in headerReq, M_DP_TB_1);
                            break;
                        case ASDUType.M_ME_TF_1 when result.Value.Value is AnalogValue analogValue1: // Значение измеряемой величины, короткий формат с плавающей запятой с меткой времени СР56Время2а
                            headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var M_ME_TF_1 = new M_ME_TF_1_Single(address, analogValue1.Value, 0, result.Value.Value.ValueDt!.Value, TimeStatus.OK);
                            length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1);
                            break;
                        default:
                            headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.UNKNOWN_TRANSFER_REASON, pn: PN.Negative, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var C_RD_NA_1 = new C_RD_NA_1(address);
                            length = C_RD_NA_1.Serialize(buffer, in headerReq, in C_RD_NA_1);
                            break;
                    }
                }
                else
                {
                    var headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                        errorTransaction ? COT.UNKNOWN_TRANSFER_REASON : COT.UNKNOWN_INFORMATION_OBJECT_ADDRESS,
                        pn: PN.Negative,
                        initAddr: header.InitAddr,
                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                    var C_RD_NA_1 = new C_RD_NA_1(address);
                    length = C_RD_NA_1.Serialize(buffer, in headerReq, in C_RD_NA_1);
                }

                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
    }
}

