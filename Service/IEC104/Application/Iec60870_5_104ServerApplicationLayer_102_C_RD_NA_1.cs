using Microsoft.Extensions.Logging;

using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    private struct AdditionInfo_C_RD_NA_1
    {
        public ASDUPacketHeader_2_2 Header { get; set; }
        public ushort Address { get; set; }
    }

    internal void Process_C_RD_NA_1(in ASDUPacketHeader_2_2 header, ushort address, CancellationToken ct)
    {
        SendInRentBuffer(static (buffer, context, additionInfo) =>
            {
                MapValueItem? result = null;
                var errorTransaction = false;
                var transactionId = (((int)additionInfo.Header.AsduType) << 24) + (additionInfo.Address << 8);

                try
                {
                    if (context._readTransactionManager.CreateTransaction(transactionId))
                    {
                        result = context._dataProvider.GetValue(additionInfo.Address);
                    }
                    else
                    {
                        errorTransaction = true;
                        context._logger.LogError("Transaction duplicate: {@type} {@address}", additionInfo.Header.AsduType, additionInfo.Address);
                    }
                }
                catch (Exception ex)
                {
                    errorTransaction = true;
                    context._logger.LogError(ex, "Transaction error: {@type} {@address}", additionInfo.Header.AsduType, additionInfo.Address);
                }
                finally
                {
                    context._readTransactionManager.DeleteTransaction(transactionId);
                }

                int length;
                var header = additionInfo.Header;

                if (result != null)
                {
                    ASDUPacketHeader_2_2 headerReq;
                    switch (result.Type)
                    {
                        case ASDUType.M_SP_TB_1 when result.Value.ValueAsBool.HasValue: // Одноэлементная информация с меткой времени СР56Время2а
                            headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                            var M_SP_TB_1 = new M_SP_TB_1_Single(additionInfo.Address, result.Value.ValueAsBool.Value ? SIQ_Value.On : SIQ_Value.Off, 0, result.Value.ValueDt!.Value, TimeStatus.OK);
                            length = M_SP_TB_1_Single.Serialize(buffer, in headerReq, M_SP_TB_1);
                            break;
                        case ASDUType.M_DP_TB_1 when result.Value.ValueAsBool.HasValue: // Двухэлементная информация с меткой времени СР56Время2а
                            headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                            var M_DP_TB_1 = new M_DP_TB_1_Single(additionInfo.Address, result.Value.ValueAsBool.Value ? DIQ_Value.On : DIQ_Value.Off, 0, result.Value.ValueDt!.Value, TimeStatus.OK);
                            length = M_DP_TB_1_Single.Serialize(buffer, in headerReq, M_DP_TB_1);
                            break;
                        case ASDUType.M_ME_TF_1 when result.Value.ValueAsFloat.HasValue: // Значение измеряемой величины, короткий формат с плавающей запятой с меткой времени СР56Время2а
                            headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                            var M_ME_TF_1 = new M_ME_TF_1_Single(additionInfo.Address, result.Value.ValueAsFloat.Value, 0, result.Value.ValueDt!.Value, TimeStatus.OK);
                            length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1);
                            break;
                        default:
                            headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.UNKNOWN_TRANSFER_REASON, pn: PN.Negative, initAddr: header.InitAddr, commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                            var C_RD_NA_1 = new C_RD_NA_1(additionInfo.Address);
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
                        commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                    var C_RD_NA_1 = new C_RD_NA_1(additionInfo.Address);
                    length = C_RD_NA_1.Serialize(buffer, in headerReq, in C_RD_NA_1);
                }

                context._packetSender!.Send(buffer.AsSpan(0, length));
            }, this, new AdditionInfo_C_RD_NA_1() { Header = header, Address = address });
    }
}

