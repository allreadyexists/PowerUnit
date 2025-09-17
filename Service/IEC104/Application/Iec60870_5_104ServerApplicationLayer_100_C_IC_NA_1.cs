using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public partial class IEC60870_5_104ServerApplicationLayer
{
    private struct AdditionInfo_C_IC_NA_1
    {
        public ASDUPacketHeader_2_2 Header { get; set; }

        public ushort Address { get; set; }
        public QOI QOI { get; set; }
        public int TransactionId { get; set; }
    }

    private void Activate(in ASDUPacketHeader_2_2 header, ushort address, QOI qoi, int transactionId, CancellationToken ct)
    {
        SendInRentBuffer(static (buffer, context, additionInfo) =>
            {
                var headerReq = new ASDUPacketHeader_2_2(additionInfo.Header.AsduType, additionInfo.Header.SQ, additionInfo.Header.Count, COT.ACTIVATE_CONFIRMATION,
                    initAddr: additionInfo.Header.InitAddr, commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress);
                var C_IC_NA_1 = new C_IC_NA_1(additionInfo.QOI);
                var length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                context._packetSender!.Send(buffer.AsSpan(0, length));

                context.SendValues2(buffer, additionInfo.Header.InitAddr, (COT)additionInfo.QOI, [.. context._dataProvider.GetGroup((byte)additionInfo.QOI)]);

                headerReq = new ASDUPacketHeader_2_2(additionInfo.Header.AsduType, additionInfo.Header.SQ, additionInfo.Header.Count, COT.ACTIVATE_COMPLETION,
                    initAddr: additionInfo.Header.InitAddr, commonAddrAsdu: context._applicationLayerOption.CommonASDUAddress, pn: PN.Positive);
                C_IC_NA_1 = new C_IC_NA_1(additionInfo.QOI);
                length = C_IC_NA_1.Serialize(buffer, in headerReq, in C_IC_NA_1);
                context._packetSender!.Send(buffer.AsSpan(0, length));

                context._readTransactionManager.DeleteTransaction(additionInfo.TransactionId);
            }, this, new AdditionInfo_C_IC_NA_1() { Header = header, Address = address, QOI = qoi, TransactionId = transactionId });
    }

    internal void Process_C_IC_NA_1(in ASDUPacketHeader_2_2 header, ushort address, QOI qoi, CancellationToken ct)
    {
        var transactionId = (((int)header.AsduType) << 24) + (address << 8) + (int)qoi;

        switch (header.CauseOfTransmit)
        {
            case COT.ACTIVATE:
                // проверить наличие уже запущенной транзакции
                // 1. нет транзакции - запускаем и регистрируем новую
                if (_readTransactionManager.CreateTransaction(transactionId))
                {
                    Activate(in header, address, qoi, transactionId, ct);
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

