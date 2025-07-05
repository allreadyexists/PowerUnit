using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Asdu;

using System.Buffers;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    private void Activate(AsduPacketHeader_2_2 header, ushort address, QOI qoi, int transactionId, CancellationToken ct, CancellationToken transactionCt)
    {
        _ = SendInRentBuffer(async (buffer) =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.ACTIVATE_CONFIRMATION, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_IC_NA_1 = new C_IC_NA_1(qoi);
                var length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                _packetSender!.Send(buffer[..length]);

                var errorTransaction = false;
                var cancelTransaction = false;
                await using var scope = _serviceProvider.CreateAsyncScope();
                {
                    var dataProvider = scope.ServiceProvider.GetRequiredService<IDataProvider>();
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, transactionCt);

                    try
                    {
                        await foreach (var groupData in dataProvider.GetDiscretGroup(_applicationLayerOption.ServerId, qoi, cts.Token))
                        {
                            switch (groupData.AsduType)
                            {
                                case AsduType.M_SP_NA_1: // Одноэлементная информация без метки времени
                                    break;
                                case AsduType.M_SP_TB_1: // Одноэлементная информация с меткой времени СР56Время2а
                                    break;
                                case AsduType.M_DP_NA_1: // Двухэлементная информация без метки времени
                                    break;
                                case AsduType.M_DP_TB_1: // Двухэлементная информация с меткой времени СР56Время2а
                                    break;
                            }
                        }

                        var M_ME_NC_1_SingleArray = ArrayPool<M_ME_NC_1_Single>.Shared.Rent(M_ME_NC_1_Single.MaxItemCount);
                        var M_ME_TF_1_SingleArray = ArrayPool<M_ME_TF_1_Single>.Shared.Rent(M_ME_TF_1_Single.MaxItemCount);

                        int countTotal = 0;
                        byte count = 0;

                        AsduType currentType = 0;
                        var currentTypeMaxCount = 0;

                        var isInit = false;

                        try
                        {
                            await foreach (var groupData in dataProvider.GetAnalogGroup(_applicationLayerOption.ServerId, qoi, cts.Token))
                            {
                                _logger.LogDebug("{@countTotal} {@type}", countTotal++, groupData.AsduType);

                                if (groupData.AsduType == AsduType.M_ME_NC_1 || groupData.AsduType == AsduType.M_ME_TF_1)
                                {
                                    if (!isInit)
                                    {
                                        currentType = groupData.AsduType;
                                        isInit = true;
                                        if (groupData.AsduType == AsduType.M_ME_NC_1)
                                        {
                                            currentTypeMaxCount = M_ME_NC_1_Single.MaxItemCount;
                                            M_ME_NC_1_SingleArray[count++] = new M_ME_NC_1_Single(groupData.Address, groupData.Value, (QDS_Status)groupData.Status);
                                        }
                                        else if (groupData.AsduType == AsduType.M_ME_TF_1)
                                        {
                                            currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;
                                            M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single(groupData.Address, groupData.Value, (QDS_Status)groupData.Status, groupData.ValueDt!.Value, 0);
                                        }
                                    }
                                    else
                                    {
                                        if (currentType != groupData.AsduType || count == currentTypeMaxCount)
                                        {
                                            headerReq = new AsduPacketHeader_2_2(currentType, SQ.Single, count, (COT)qoi, initAddr: header.InitAddr,
                                                            commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                            if (currentType == AsduType.M_ME_NC_1)
                                            {
                                                length = M_ME_NC_1_Single.Serialize(buffer, in headerReq, M_ME_NC_1_SingleArray, count);
                                            }
                                            else if (currentType == AsduType.M_ME_TF_1)
                                            {
                                                length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1_SingleArray, count);
                                            }

                                            _logger.LogDebug("!!! {@send} {@type}", count, currentType);
                                            _packetSender!.Send(buffer[..length]);

                                            if (groupData.AsduType == AsduType.M_ME_NC_1)
                                                currentTypeMaxCount = M_ME_NC_1_Single.MaxItemCount;
                                            else if (groupData.AsduType == AsduType.M_ME_TF_1)
                                                currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;

                                            currentType = groupData.AsduType;
                                            count = 0;
                                        }

                                        if (groupData.AsduType == AsduType.M_ME_NC_1)
                                        {
                                            M_ME_NC_1_SingleArray[count++] = new M_ME_NC_1_Single(groupData.Address, groupData.Value, (QDS_Status)groupData.Status);
                                        }
                                        else if (groupData.AsduType == AsduType.M_ME_TF_1)
                                        {
                                            M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single(groupData.Address, groupData.Value, (QDS_Status)groupData.Status, groupData.ValueDt!.Value, 0);
                                        }
                                    }
                                }
                            }

                            if (count > 0)
                            {
                                headerReq = new AsduPacketHeader_2_2(currentType, SQ.Single, count, (COT)qoi, initAddr: header.InitAddr,
                                                            commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                if (currentType == AsduType.M_ME_NC_1)
                                {
                                    length = M_ME_NC_1_Single.Serialize(buffer, in headerReq, M_ME_NC_1_SingleArray, count);
                                    currentTypeMaxCount = M_ME_NC_1_Single.MaxItemCount;
                                }
                                else if (currentType == AsduType.M_ME_TF_1)
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
                            ArrayPool<M_ME_NC_1_Single>.Shared.Return(M_ME_NC_1_SingleArray);
                            ArrayPool<M_ME_TF_1_Single>.Shared.Return(M_ME_TF_1_SingleArray);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // отмена транзакции
                        if (transactionCt.IsCancellationRequested)
                        {
                            cancelTransaction = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Transaction error");
                        errorTransaction = true;
                    }
                }

                if (!cancelTransaction)
                {
                    headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.ACTIVATE_COMPLETION, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress, pn: errorTransaction ? PN.Negative : PN.Positive);
                    C_IC_NA_1 = new C_IC_NA_1(qoi);
                    length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                    _packetSender!.Send(buffer[..length]);
                    _readTransactionManager.DeleteTransaction(transactionId);
                }
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
                }
                else
                {
                    // 2. есть транзакция - подтвердить, но не исполнять, а поставить а очередь на исполнение?
                    // или просто промолчать - тут вопрос к поведению клиента, зачем он прислал еще один запрос на чтение
                    // если не получил ответ на первый?
                }

                break;
            case COT.DEACTIVATE:
                // 1. отметить транзакцию - если она была
                var result = _readTransactionManager.DeleteTransaction(transactionId);
                // 2. отправить DEACTIVATE_CONFIRMATION
                Deactivate(header, address, qoi, result, ct);
                break;
        }
    }
}

