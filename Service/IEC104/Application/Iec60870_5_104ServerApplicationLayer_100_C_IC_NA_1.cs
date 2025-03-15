using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    private void Activate(ASDUPacketHeader_2_2 header, ushort address, QOI qoi, int transactionId, CancellationToken ct)
    {
        _ = SendInRentBuffer((buffer) =>
            {
                var headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.ACTIVATE_CONFIRMATION, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var C_IC_NA_1 = new C_IC_NA_1(qoi);
                var length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                _packetSender!.Send(buffer[..length]);

                SendValues(buffer, header.InitAddr, (COT)qoi, _dataProvider.GetGroup((byte)qoi));

                headerReq = new ASDUPacketHeader_2_2(header.AsduType, header.SQ, header.Count, COT.ACTIVATE_COMPLETION, initAddr: header.InitAddr, commonAddrAsdu: _applicationLayerOption.CommonASDUAddress, pn: PN.Positive);
                C_IC_NA_1 = new C_IC_NA_1(qoi);
                length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                _packetSender!.Send(buffer[..length]);

                _readTransactionManager.DeleteTransaction(transactionId);

                return Task.CompletedTask;
            });
    }

    internal void Process_C_IC_NA_1(ASDUPacketHeader_2_2 header, ushort address, QOI qoi, CancellationToken ct)
    {
        var transactionId = (((int)header.AsduType) << 24) + (address << 8) + (int)qoi;

        switch (header.CauseOfTransmit)
        {
            case COT.ACTIVATE:
                // проверить наличие уже запущенной транзакции
                // 1. нет транзакции - запускаем и регистрируем новую
                if (_readTransactionManager.CreateTransaction(transactionId))
                {
                    Activate(header, address, qoi, transactionId, ct);
                    break;
                }
                else
                {
                    // 2. есть транзакция - подтвердить, но не исполнять, а поставить а очередь на исполнение?
                    // или просто промолчать - тут вопрос к поведению клиента, зачем он прислал еще один запрос на чтение
                    // если не получил ответ на первый?
                    throw new IEC60870_5_104ApplicationException(header);
                }
            default:
                throw new IEC60870_5_104ApplicationException(header);
        }
    }
}

