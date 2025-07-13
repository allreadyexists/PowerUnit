using Microsoft.Extensions.Logging;

using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    //private Task<AnalogValue?> GetAnalogValue(ushort address, CancellationToken ct)
    //{
    //    using var scope = _serviceProvider.CreateAsyncScope();
    //    {
    //        var dataProvider = scope.ServiceProvider.GetRequiredService<IDataProvider>();
    //        return dataProvider.GetAnalogValue(_applicationLayerOption.ServerId, address, ct);
    //    }
    //}

    //private Task<DiscretValue?> GetDiscretValue(ushort address, CancellationToken ct)
    //{
    //    using var scope = _serviceProvider.CreateAsyncScope();
    //    {
    //        var dataProvider = scope.ServiceProvider.GetRequiredService<IDataProvider>();
    //        return dataProvider.GetDiscretValue(_applicationLayerOption.ServerId, address, ct);
    //    }
    //}

    internal void Process_C_RD_NA_1(AsduPacketHeader_2_2 header, ushort address, CancellationToken ct)
    {
        _ = SendInRentBuffer((buffer) =>
            {
                (byte Type, BaseValue Value)? result = null;
                var errorTransaction = false;
                var transactionId = (((int)header.AsduType) << 24) + (address << 8);

                try
                {
                    if (_readTransactionManager.CreateTransaction(transactionId, out var transactionCt))
                    {
                        _values.TryGetValue(address, out var result1);

                        //var getAnalogValueTask = GetAnalogValue(address, ct);
                        //var getDiscretValueTask = GetDiscretValue(address, ct);

                        //await Task.WhenAll(getAnalogValueTask, getDiscretValueTask);

                        //if (getAnalogValueTask.Result != null)
                        //    result = getAnalogValueTask.Result;
                        //else if (getDiscretValueTask.Result != null)
                        //    result = getDiscretValueTask.Result;
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
                    AsduPacketHeader_2_2 headerReq;
                    switch ((AsduType)result.Value.Type)
                    {
                        //case AsduType.M_ME_NC_1 when result is AnalogValue analogValue1: // Значение измеряемой величины, короткий формате плавающей запятой
                        //    headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                        //        COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                        //    var M_ME_NC_1 = new M_ME_NC_1_Single(result.Address, analogValue1.Value, 0);
                        //    length = M_ME_NC_1_Single.Serialize(buffer, in headerReq, [M_ME_NC_1]);
                        //    break;
                        case AsduType.M_ME_TF_1 when result.Value.Value is AnalogValue analogValue2: // Значение измеряемой величины, короткий формат с плавающей запятой с меткой времени СР56Время2а
                            headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var M_ME_TF_1 = new M_ME_TF_1_Single(address, analogValue2.Value, 0, result.Value.Value.ValueDt!.Value, TimeStatus.OK);
                            length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, [M_ME_TF_1]);
                            break;

                        //case AsduType.M_SP_NA_1 when result is DiscretValue discretValue1: // Одноэлементная информация без метки времени
                        //    headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                        //                                    COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                        //    var M_SP_NA_1 = new M_SP_NA_1_Single(result.Address, discretValue1.Value ? SIQ_Value.On : SIQ_Value.Off, 0);
                        //    length = M_SP_NA_1_Single.Serialize(buffer, in headerReq, in M_SP_NA_1);
                        //    break;
                        //case AsduType.M_SP_TB_1 when result is DiscretValue discretValue2: // Одноэлементная информация с меткой времени СР56Время2а
                        //    break;
                        //case AsduType.M_DP_NA_1 when result is DiscretValue discretValue3: // Двухэлементная информация без метки времени
                        //    headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                        //                                    COT.REQUEST_REQUESTED_DATA, pn: PN.Positive, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                        //    var M_DP_NA_1 = new M_DP_NA_1_Single(result.Address, discretValue3.Value ? DIQ_Value.On : DIQ_Value.Off, 0);
                        //    length = M_DP_NA_1_Single.Serialize(buffer, in headerReq, in M_DP_NA_1);
                        //    break;
                        //case AsduType.M_DP_TB_1 when result is DiscretValue discretValue4: // Двухэлементная информация с меткой времени СР56Время2а
                        //    break;

                        default:
                            headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                                COT.UNKNOWN_TRANSFER_REASON, pn: PN.Negative, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var C_RD_NA_1 = new C_RD_NA_1(address);
                            length = C_RD_NA_1.Serialize(buffer, in headerReq, in C_RD_NA_1);
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
                    length = C_RD_NA_1.Serialize(buffer, in headerReq, in C_RD_NA_1);
                }

                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
    }
}

