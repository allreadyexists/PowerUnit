using Microsoft.Extensions.Logging;

using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

using System.Buffers;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{


    private void Activate(AsduPacketHeader_2_2 header, ushort address, QOI qoi, int transactionId, CancellationToken ct, CancellationToken transactionCt)
    {
        _ = SendInRentBuffer((buffer) =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.ACTIVATE_CONFIRMATION, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_IC_NA_1 = new C_IC_NA_1(qoi);
                var length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                _packetSender!.Send(buffer[..length]);
                _logger.LogInformation("Send ack {@qoi}", qoi);

                var errorTransaction = false;
                var cancelTransaction = false;

                if (_groups.TryGetValue((int)qoi, out var group))
                {
                    var M_SP_TB_1_SingleArray = ArrayPool<M_SP_TB_1_Single>.Shared.Rent(M_SP_TB_1_Single.MaxItemCount);
                    var M_DP_TB_1_SingleArray = ArrayPool<M_DP_TB_1_Single>.Shared.Rent(M_DP_TB_1_Single.MaxItemCount);
                    var M_ME_TF_1_SingleArray = ArrayPool<M_ME_TF_1_Single>.Shared.Rent(M_ME_TF_1_Single.MaxItemCount);

                    int countTotal = 0;
                    byte count = 0;

                    byte currentType = 0;
                    var currentTypeMaxCount = 0;

                    var isInit = false;

                    try
                    {
                        foreach (var groupData in _values.Where(x => group.Contains(x.Key)))
                        {
                            _logger.LogDebug("{@countTotal} {@type}", countTotal++, groupData.Value.Type);

                            if (groupData.Value.Type == (byte)AsduType.M_SP_TB_1 ||
                                groupData.Value.Type == (byte)AsduType.M_DP_TB_1 ||
                                groupData.Value.Type == (byte)AsduType.M_ME_TF_1)
                            {
                                if (!isInit)
                                {
                                    currentType = groupData.Value.Type;
                                    isInit = true;
                                    if (groupData.Value.Type == (byte)AsduType.M_SP_TB_1)
                                    {
                                        currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                                        M_SP_TB_1_SingleArray[count++] = new M_SP_TB_1_Single(groupData.Key,
                                            ((DiscretValue)groupData.Value.Value).Value ? SIQ_Value.On : SIQ_Value.Off, 0,
                                            ((DiscretValue)groupData.Value.Value).ValueDt!.Value, 0);
                                    }
                                    else if (groupData.Value.Type == (byte)AsduType.M_DP_TB_1)
                                    {
                                        currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                                        M_DP_TB_1_SingleArray[count++] = new M_DP_TB_1_Single(groupData.Key,
                                            ((DiscretValue)groupData.Value.Value).Value ? DIQ_Value.On : DIQ_Value.Off, 0,
                                            ((DiscretValue)groupData.Value.Value).ValueDt!.Value, 0);
                                    }
                                    else if (groupData.Value.Type == (byte)AsduType.M_ME_TF_1)
                                    {
                                        currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;
                                        M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single(groupData.Key,
                                            ((AnalogValue)groupData.Value.Value).Value, 0,
                                            ((AnalogValue)groupData.Value.Value).ValueDt!.Value, 0);
                                    }
                                }
                                else
                                {
                                    if (currentType != groupData.Value.Type || count == currentTypeMaxCount)
                                    {
                                        headerReq = new AsduPacketHeader_2_2((AsduType)currentType, SQ.Single, count, (COT)qoi, initAddr: header.InitAddr,
                                                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                        if (currentType == (byte)AsduType.M_SP_TB_1)
                                        {
                                            length = M_SP_TB_1_Single.Serialize(buffer, in headerReq, M_SP_TB_1_SingleArray, count);
                                        }
                                        else if (currentType == (byte)AsduType.M_DP_TB_1)
                                        {
                                            length = M_DP_TB_1_Single.Serialize(buffer, in headerReq, M_DP_TB_1_SingleArray, count);
                                        }
                                        else if (currentType == (byte)AsduType.M_ME_TF_1)
                                        {
                                            length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1_SingleArray, count);
                                        }

                                        _logger.LogDebug("!!! {@send} {@type}", count, currentType);
                                        _packetSender!.Send(buffer[..length]);

                                        if (groupData.Value.Type == (byte)AsduType.M_SP_TB_1)
                                            currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                                        if (groupData.Value.Type == (byte)AsduType.M_DP_TB_1)
                                            currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                                        else if (groupData.Value.Type == (byte)AsduType.M_ME_TF_1)
                                            currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;

                                        currentType = groupData.Value.Type;
                                        count = 0;
                                    }

                                    if (groupData.Value.Type == (byte)AsduType.M_SP_TB_1)
                                    {
                                        M_SP_TB_1_SingleArray[count++] = new M_SP_TB_1_Single(groupData.Key,
                                            ((DiscretValue)groupData.Value.Value).Value ? SIQ_Value.On : SIQ_Value.Off, 0,
                                            ((DiscretValue)groupData.Value.Value).ValueDt!.Value, 0);
                                    }
                                    else if (groupData.Value.Type == (byte)AsduType.M_DP_TB_1)
                                    {
                                        M_DP_TB_1_SingleArray[count++] = new M_DP_TB_1_Single(groupData.Key,
                                            ((DiscretValue)groupData.Value.Value).Value ? DIQ_Value.On : DIQ_Value.Off, 0,
                                            ((DiscretValue)groupData.Value.Value).ValueDt!.Value, 0);
                                    }
                                    else if (groupData.Value.Type == (byte)AsduType.M_ME_TF_1)
                                    {
                                        M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single(groupData.Key,
                                            ((AnalogValue)groupData.Value.Value).Value, 0,
                                            ((AnalogValue)groupData.Value.Value).ValueDt!.Value, 0);
                                    }
                                }
                            }
                        }

                        if (count > 0)
                        {
                            headerReq = new AsduPacketHeader_2_2((AsduType)currentType, SQ.Single, count, (COT)qoi, initAddr: header.InitAddr,
                                                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            if (currentType == (byte)AsduType.M_SP_TB_1)
                            {
                                length = M_SP_TB_1_Single.Serialize(buffer, in headerReq, M_SP_TB_1_SingleArray, count);
                                currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                            }
                            else if (currentType == (byte)AsduType.M_DP_TB_1)
                            {
                                length = M_DP_TB_1_Single.Serialize(buffer, in headerReq, M_DP_TB_1_SingleArray, count);
                                currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                            }
                            else if (currentType == (byte)AsduType.M_ME_TF_1)
                            {
                                length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1_SingleArray, count);
                                currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;
                            }

                            _logger.LogDebug("!!! {@send} {@type}", count, currentType);
                            _packetSender!.Send(buffer[..length]);
                        }
                    }
                    finally
                    {
                        ArrayPool<M_SP_TB_1_Single>.Shared.Return(M_SP_TB_1_SingleArray);
                        ArrayPool<M_DP_TB_1_Single>.Shared.Return(M_DP_TB_1_SingleArray);
                        ArrayPool<M_ME_TF_1_Single>.Shared.Return(M_ME_TF_1_SingleArray);
                    }
                }

                if (!cancelTransaction)
                {
                    headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.ACTIVATE_COMPLETION, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress, pn: errorTransaction ? PN.Negative : PN.Positive);
                    C_IC_NA_1 = new C_IC_NA_1(qoi);
                    length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                    _packetSender!.Send(buffer[..length]);
                    _logger.LogInformation("Send complite {@qoi}", qoi);
                    _readTransactionManager.DeleteTransaction(transactionId);
                }

                return Task.CompletedTask;
            });
    }

    private void Deactivate(AsduPacketHeader_2_2 header, ushort address, QOI qoi, bool isDeactivate, CancellationToken ct)
    {
        _ = SendInRentBuffer(buffer =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count,
                COT.DEACTIVATE_CONFIRMATION,
                pn: isDeactivate ? PN.Positive : PN.Negative,
                initAddr: header.InitAddr,
                commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_IC_NA_1 = new C_IC_NA_1(qoi);
                var length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
    }

    internal void Process_C_IC_NA_1(AsduPacketHeader_2_2 header, ushort address, QOI qoi, CancellationToken ct)
    {
        var transactionId = (((int)header.AsduType) << 24) + (address << 8) + (int)qoi;

        switch (header.CauseOfTransmit)
        {
            case COT.ACTIVATE:
                // проверить наличие уже запущенной транзакции
                // 1. нет транзакции - запускаем и регистрируем новую
                if (_readTransactionManager.CreateTransaction(transactionId, out var transactionCt))
                {
                    Activate(header, address, qoi, transactionId, ct, transactionCt);
                    break;
                }
                else
                {
                    // 2. есть транзакция - подтвердить, но не исполнять, а поставить а очередь на исполнение?
                    // или просто промолчать - тут вопрос к поведению клиента, зачем он прислал еще один запрос на чтение
                    // если не получил ответ на первый?
                    throw new Iec60870_5_104ApplicationException(header);
                }
            case COT.DEACTIVATE:
                // 1. отметить транзакцию - если она была
                var result = _readTransactionManager.DeleteTransaction(transactionId);
                // 2. отправить DEACTIVATE_CONFIRMATION
                Deactivate(header, address, qoi, result, ct);
                break;
            default:
                throw new Iec60870_5_104ApplicationException(header);
        }
    }
}

