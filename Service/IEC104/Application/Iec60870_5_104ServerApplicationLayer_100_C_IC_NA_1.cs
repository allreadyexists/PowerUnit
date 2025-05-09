using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PowerUnit.Asdu;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    private void Activate(AsduPacketHeader_2_2 header, ushort address, QOI qoi, int transactionId, CancellationToken ct, CancellationToken transactionCt)
    {
        _ = SendInRentBuffer(async (buffer) =>
            {
                var headerReq = new AsduPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.ACTIVATE_CONFIRMATION, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_IC_NA_1 = new C_IC_NA_1(qoi);
                var length = C_IC_NA_1.Serialize(buffer, ref headerReq, ref C_IC_NA_1);
                _packetSender!.Send(buffer[..length]);

                var errorTransaction = false;
                var cancelTransaction = false;
                using var scope = _serviceProvider.CreateAsyncScope();
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
                                case AsduType.M_SP_TA_1: // Одноэлементная информация с меткой времени
                                    break;
                                case AsduType.M_DP_NA_1: // Двухэлементная информация без метки времени
                                    break;
                                case AsduType.M_DP_TA_1: // Двухэлементная информация с меткой времени
                                    headerReq = new AsduPacketHeader_2_2(groupData.AsduType, SQ.Single, 1, (COT)qoi, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                    length = M_DP_TA_1_Single.Serialize(buffer, ref headerReq, [new M_DP_TA_1_Single(
                                        groupData.Address, groupData.Value ? DIQ_Value.On : DIQ_Value.Off, (DIQ_Status)groupData.Status, TimeOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime))]);

                                    _packetSender!.Send(buffer[..length]);
                                    break;
                                case AsduType.M_SP_TB_1: // Одноэлементная информация с меткой времени СР56Время2а
                                case AsduType.M_DP_TB_1: // Двухэлементная информация с меткой времени СР56Время2а
                                    break;
                            }
                        }

                        await foreach (var groupData in dataProvider.GetAnalogGroup(_applicationLayerOption.ServerId, qoi, cts.Token))
                        {
                            switch (groupData.AsduType)
                            {
                                case AsduType.M_ME_NC_1: // Значение измеряемой величины, короткий формате плавающей запятой
                                    headerReq = new AsduPacketHeader_2_2(groupData.AsduType, SQ.Single, 1, (COT)qoi, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                    length = M_ME_NC_1_Single.Serialize(buffer, ref headerReq, [new M_ME_NC_1_Single(groupData.Address, groupData.Value, (QDS_Status)groupData.Status)]);
                                    _packetSender!.Send(buffer[..length]);
                                    break;
                                case AsduType.M_ME_TC_1: // Значение измеряемой величины, короткий формате с плавающей запятой с меткой времени
                                    headerReq = new AsduPacketHeader_2_2(groupData.AsduType, SQ.Single, 1, (COT)qoi, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                    length = M_ME_TC_1_Single.Serialize(buffer, ref headerReq, [new M_ME_TC_1_Single(groupData.Address, groupData.Value, (QDS_Status)groupData.Status, groupData.ValueDt!.Value, 0)]);
                                    _packetSender!.Send(buffer[..length]);
                                    break;
                                case AsduType.M_ME_TF_1: // Значение измеряемой величины, короткий формат с плавающей запятой с меткой времени СР56Время2а
                                    headerReq = new AsduPacketHeader_2_2(groupData.AsduType, SQ.Single, 1, (COT)qoi, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                    length = M_ME_TF_1_Single.Serialize(buffer, ref headerReq, [new M_ME_TF_1_Single(groupData.Address, groupData.Value, (QDS_Status)groupData.Status, groupData.ValueDt!.Value, 0)]);
                                    _packetSender!.Send(buffer[..length]);
                                    break;
                            }
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
                    length = C_IC_NA_1.Serialize(buffer, ref headerReq, ref C_IC_NA_1);
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
                var length = C_IC_NA_1.Serialize(buffer, ref headerReq, ref C_IC_NA_1);
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

