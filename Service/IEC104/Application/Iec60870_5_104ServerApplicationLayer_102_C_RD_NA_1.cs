using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Asdu;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    internal void Process_C_RD_NA_1(AsduPacketHeader_2_2 header, ushort address, CancellationToken ct)
    {
        _ = SendInRentBuffer(async (buffer) =>
            {
                AnalogValue? result = null;
                var errorTransaction = false;
                var transactionId = (((int)header.AsduType) << 24) + (address << 8);

                try
                {
                    if (_readTransactionManager.CreateTransaction(transactionId, out var transactionCt))
                    {
                        using var scope = _serviceProvider.CreateAsyncScope();
                        {
                            var dataProvider = scope.ServiceProvider.GetRequiredService<IDataProvider>();
                            result = await dataProvider.GetAnalogValue(_applicationLayerOption.ServerId, address, ct);
                        }
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

                if (result != null)
                {
                    AsduPacketHeader_2_2 headerReq;
                    switch (result.AsduType)
                    {
                        case AsduType.M_ME_NC_1: // Значение измеряемой величины, короткий формате плавающей запятой
                            headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var M_ME_NC_1 = new M_ME_NC_1_Single(result.Address, result.Value, 0);
                            length = M_ME_NC_1_Single.Serialize(buffer, ref headerReq, [M_ME_NC_1]);
                            break;
                        case AsduType.M_ME_TC_1: // Значение измеряемой величины, короткий формате с плавающей запятой с меткой времени
                            headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var M_ME_TC_1 = new M_ME_TC_1_Single(result.Address, result.Value, 0, result.ValueDt!.Value, (TimeStatus)result.Status);
                            length = M_ME_TC_1_Single.Serialize(buffer, ref headerReq, [M_ME_TC_1]);
                            break;
                        case AsduType.M_ME_TF_1: // Значение измеряемой величины, короткий формат с плавающей запятой с меткой времени СР56Время2а
                            headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var M_ME_TF_1 = new M_ME_TF_1_Single(result.Address, result.Value, 0, result.ValueDt!.Value, (TimeStatus)result.Status);
                            length = M_ME_TF_1_Single.Serialize(buffer, ref headerReq, [M_ME_TF_1]);
                            break;
                        default:
                            headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.UNKNOWN_TRANSFER_REASON, pn: PN.Negative, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var C_RD_NA_1 = new C_RD_NA_1(address);
                            length = C_RD_NA_1.Serialize(buffer, ref headerReq, ref C_RD_NA_1);
                            break;
                    }
                }
                else
                {
                    var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                        errorTransaction ? COT.UNKNOWN_TRANSFER_REASON : COT.UNKNOWN_INFORMATION_OBJECT_ADDRESS,
                        pn: PN.Negative,
                        initAddr: header.InitAddr,
                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                    var C_RD_NA_1 = new C_RD_NA_1(address);
                    length = C_RD_NA_1.Serialize(buffer, ref headerReq, ref C_RD_NA_1);
                }

                _packetSender!.Send(buffer[..length]);
            });
    }
}

