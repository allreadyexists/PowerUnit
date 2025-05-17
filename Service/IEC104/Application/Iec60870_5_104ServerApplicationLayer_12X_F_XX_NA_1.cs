using PowerUnit.Asdu;

using System.Collections.Concurrent;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    private readonly ConcurrentDictionary<ushort, FileReadCache> _fileReadStates = [];
    private readonly ConcurrentDictionary<ushort, FileWriteCache> _fileWriteStates = [];

    /// <summary>
    /// Готовность файла F_FR_NA_1 = 120,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="lof"></param>
    /// <param name="frq"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_FR_NA_1(AsduPacketHeader_2_2 header, ushort address, ushort nof, uint lof, FRQ frq, CancellationToken ct)
    {
        if (frq == FRQ.Positive && lof > 0)
        {
            _ = SendInRentBuffer(buffer =>
                {
                    var fileReady = _fileProvider.PrepareWriteFile(nof, out var fileInfo);
                    if (fileReady)
                    {
                        _fileWriteStates[nof] = new FileWriteCache();
                    }

                    var headerReq = new AsduPacketHeader_2_2(AsduType.F_SC_NA_1, SQ.Single, 1,
                        COT.FILE_TRANSFER,
                        fileReady ? PN.Positive : PN.Negative,
                        initAddr: header.InitAddr,
                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                    var F_SC_NA_1 = new F_SC_NA_1(address, nof, 1, SCQ.FileRequest);
                    var length = F_SC_NA_1.Serialize(buffer, in headerReq, in F_SC_NA_1);
                    _packetSender!.Send(buffer[..length]);
                    return Task.CompletedTask;
                });
        }
        else
        {
            throw new Iec60870_5_104ApplicationException(header, $"address {address} nof {nof} lof {lof} frq {frq}");
        }
    }

    /// <summary>
    /// Готовность секции F_SR_NA_1 = 121,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="los"></param>
    /// <param name="srq"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_SR_NA_1(AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, uint los, SRQ srq, CancellationToken ct)
    {
        if (srq == SRQ.Ready && los > 0)
        {
            _ = SendInRentBuffer(buffer =>
            {
                var fileReady = _fileWriteStates.TryGetValue(nof, out _);

                var headerReq = new AsduPacketHeader_2_2(AsduType.F_SC_NA_1, SQ.Single, 1,
                    COT.FILE_TRANSFER,
                    fileReady ? PN.Positive : PN.Negative,
                    initAddr: header.InitAddr,
                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var F_SC_NA_1 = new F_SC_NA_1(address, nof, nos, SCQ.SectionRequest);
                var length = F_SC_NA_1.Serialize(buffer, in headerReq, in F_SC_NA_1);
                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
        }
        else
        {
            throw new Iec60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} los {los} srq {srq}");
        }
    }

    /// <summary>
    /// Вызов директории, выбор файла, вызов файла, вызов секции F_SC_NA_1 = 122,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="scq"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_SC_NA_1(AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, SCQ scq, CancellationToken ct)
    {
        switch (header.CauseOfTransmit)
        {
            case COT.REQUEST_REQUESTED_DATA when scq == SCQ.FileSelect: // directory only
                _ = SendInRentBuffer(buffer =>
                    {
                        var content = _fileProvider.GetDirectoryContent(address, nof);

                        if (content.Any())
                        {
                            var directoryContentChanks = content.Chunk(F_DR_TA_1_Sequence.MaxItemCount);
                            var F_DR_TA_1_Sequences = new F_DR_TA_1_Sequence[F_DR_TA_1_Sequence.MaxItemCount];

                            foreach (var directoryContentChank in directoryContentChanks)
                            {
                                var headerReq = new AsduPacketHeader_2_2(AsduType.F_DR_TA_1, SQ.Sequence, (byte)directoryContentChank.Length,
                                COT.REQUEST_REQUESTED_DATA,
                                PN.Positive,
                                initAddr: header.InitAddr,
                                commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                for (var i = 0; i < directoryContentChank.Length; i++)
                                {
                                    F_DR_TA_1_Sequences[i] = new F_DR_TA_1_Sequence(
                                        directoryContentChank[i].FileOrSubDirectoryName,
                                        directoryContentChank[i].FileSize,
                                        directoryContentChank[i].SOF,
                                        directoryContentChank[i].FileOrDirectoryTimeStamp);
                                }

                                var length = F_DR_TA_1_Sequence.Serialize(buffer, in headerReq, new Address3(address), F_DR_TA_1_Sequences[0..directoryContentChank.Length]);
                                _packetSender!.Send(buffer[..length]);
                                address += (ushort)directoryContentChank.Length;
                            }
                        }
                        else
                        {
                            var headerReq = new AsduPacketHeader_2_2(AsduType.F_DR_TA_1, SQ.Sequence, 1,
                                COT.REQUEST_REQUESTED_DATA,
                                PN.Positive,
                                initAddr: header.InitAddr,
                                commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                            var length = F_DR_TA_1_Sequence.Serialize(buffer, in headerReq, new Address3(address), []);
                            _packetSender!.Send(buffer[..length]);
                        }

                        return Task.CompletedTask;
                    });
                break;
            case COT.FILE_TRANSFER: // file only
                _ = SendInRentBuffer(buffer =>
                    {
                        switch (scq)
                        {
                            case SCQ.FileSelect: // инициализация чтения файла
                                                 // ответ IEC 60870-5-101/104 ASDU: ASDU=1 F_FR_NA_1 File    IOA=0 'file ready'
                                var headerReq = new AsduPacketHeader_2_2(AsduType.F_FR_NA_1, SQ.Single, 1,
                                    COT.FILE_TRANSFER,
                                    PN.Positive,
                                    initAddr: header.InitAddr,
                                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                var fileReady = _fileProvider.PrepareReadFile(nof, out var fileInfo);
                                if (fileReady)
                                {
                                    _fileReadStates[nof] = fileInfo!;
                                }

                                var F_FR_NA_1 = new F_FR_NA_1(address, nof, fileReady ? (uint)fileInfo!.Length : 0, fileReady ? FRQ.Positive : FRQ.Negative);
                                var length = F_FR_NA_1.Serialize(buffer, in headerReq, in F_FR_NA_1);

                                _packetSender!.Send(buffer[..length]);

                                break;
                            case SCQ.FileRequest: // запрос секции
                                //ответ IEC 60870 - 5 - 101 / 104 ASDU: ASDU = 1 F_SR_NA_1 File    IOA = 0 'section ready'
                                F_SR_NA_1 F_SR_NA_1;
                                int length1;

                                var fileReady1 = _fileReadStates.TryGetValue(nof, out var fileCache);

                                var headerReq1 = new AsduPacketHeader_2_2(AsduType.F_SR_NA_1, SQ.Single, 1,
                                    COT.FILE_TRANSFER,
                                    PN.Positive,
                                    initAddr: header.InitAddr,
                                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                if (fileReady1)
                                {
                                    F_SR_NA_1 = new F_SR_NA_1(address, nof, nos, (ushort)fileCache!.Length, SRQ.Ready);
                                }
                                else
                                {
                                    F_SR_NA_1 = new F_SR_NA_1(address, nof, nos, 0, SRQ.NotReady);
                                }

                                length1 = F_SR_NA_1.Serialize(buffer, in headerReq1, in F_SR_NA_1);
                                _packetSender!.Send(buffer[..length1]);
                                break;
                            case SCQ.SectionRequest: // запрос секции
                                                     // ответ - готовые сегменты IEC 60870-5-101/104 ASDU: ASDU=1 F_SG_NA_1 File    IOA=0 'segment'
                                                     // когда сегменты закончатся: IEC 60870 - 5 - 101 / 104 ASDU: ASDU = 1 F_LS_NA_1 File    IOA = 0 'last section, last segment'
                                var fileReady2 = _fileReadStates.TryGetValue(nof, out var fileCache1);

                                if (fileReady2)
                                {
                                    var (section, cs) = fileCache1!.Sections[nos - 1];
                                    foreach (var segment in section.Chunk((byte)_applicationLayerOption.FileSegmentSize))
                                    {
                                        var headerReq2 = new AsduPacketHeader_2_2(AsduType.F_SG_NA_1, SQ.Single, 1,
                                            COT.FILE_TRANSFER,
                                            PN.Positive,
                                            initAddr: header.InitAddr,
                                            commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                        var F_SG_NA_1 = new F_SG_NA_1(address, nof, nos, (byte)segment.Length);

                                        var length2 = F_SG_NA_1.Serialize(buffer, in headerReq2, in F_SG_NA_1, segment);
                                        _packetSender!.Send(buffer[..length2]);
                                    }

                                    var headerReq3 = new AsduPacketHeader_2_2(AsduType.F_LS_NA_1, SQ.Single, 1,
                                            COT.FILE_TRANSFER,
                                            PN.Positive,
                                            initAddr: header.InitAddr,
                                            commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                    var F_LS_NA_1 = new F_LS_NA_1(address, nof, nos, LSQ.SectionSendWithoutDeactivation, cs);

                                    var length3 = F_LS_NA_1.Serialize(buffer, in headerReq3, in F_LS_NA_1);
                                    _packetSender!.Send(buffer[..length3]);
                                }
                                break;
                            default:
                                throw new Iec60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} scq {scq}");
                        }

                        return Task.CompletedTask;
                    });
                break;
            default:
                throw new Iec60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} scq {scq}");
        }
    }

    /// <summary>
    /// Последняя секция, последний сегмент F_LS_NA_1 = 123,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="lsq"></param>
    /// <param name="chs"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_LS_NA_1(AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, LSQ lsq, byte chs, CancellationToken ct)
    {
        _ = SendInRentBuffer(buffer =>
            {
                var fileReady = _fileWriteStates.TryGetValue(nof, out var fileCache);
                var isFile = lsq is LSQ.FileSendWithoutDeactivation or LSQ.FileSendWithDeactivation;

                AFQ afq = AFQ.Default;

                if (fileReady)
                {
                    if (isFile)
                    {
                        var fileCs = fileCache.Sections.SelectMany(x => x.Value).Aggregate<byte[], byte>(0, (x, t) => (byte)(x + t.Aggregate<byte, byte>(0, (xx, tt) => (byte)(xx + tt))));
                        if (fileCs == chs)
                        {
                            afq = AFQ.FileConfirmPositive;
                            _fileProvider.CompliteWriteFile(nof, fileCache.Sections.SelectMany(x => x.Value));
                        }
                        else
                        {
                            afq = AFQ.ErrorCS;
                        }
                    }
                    else
                    {
                        // check section CS
                        if (fileCache.Sections.TryGetValue(nos, out var section))
                        {
                            var sectionCs = section.Aggregate<byte[], byte>(0, (x, t) => (byte)(x + t.Aggregate<byte, byte>(0, (xx, tt) => (byte)(xx + tt))));
                            if (sectionCs == chs)
                            {
                                afq = AFQ.SectionConfirmPositive;
                            }
                            else
                            {
                                afq = AFQ.ErrorCS;
                            }
                        }
                        else
                        {
                            afq = AFQ.UndefinedSectionName;
                        }
                    }
                }
                else
                {
                    afq = AFQ.UndefinedFileName;
                }

                var headerReq = new AsduPacketHeader_2_2(AsduType.F_AF_NA_1, SQ.Single, 1,
                    COT.FILE_TRANSFER,
                    PN.Positive,
                    initAddr: header.InitAddr,
                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var F_AF_NA_1 = new F_AF_NA_1(address, nof, nos, afq);
                var length = F_AF_NA_1.Serialize(buffer, in headerReq, in F_AF_NA_1);
                _packetSender!.Send(buffer[..length]);
                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Подтверждение приема файла, подтверждение приема секции F_AF_NA_1 = 124,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="afq"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Process_F_AF_NA_1(AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, AFQ afq, CancellationToken ct)
    {
        _ = SendInRentBuffer(buffer =>
            {
                switch (afq)
                {
                    case AFQ.SectionConfirmPositive:
                        // продолжить передачу секций если они есть
                        var newNos = nos + 1;
                        AsduPacketHeader_2_2 headerReq1;
                        int length1;

                        var fileReady = _fileReadStates.TryGetValue(nof, out var fileCache);

                        if (fileReady)
                        {
                            if (newNos <= fileCache!.Sections.Count)
                            {
                                try
                                {
                                    var section = fileCache!.Sections[newNos - 1];

                                    headerReq1 = new AsduPacketHeader_2_2(AsduType.F_SR_NA_1, SQ.Single, 1,
                                    COT.FILE_TRANSFER,
                                    PN.Positive,
                                    initAddr: header.InitAddr,
                                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                    var F_SR_NA_1 = new F_SR_NA_1(address, nof, (byte)newNos, (uint)section.section.Length, SRQ.Ready);

                                    length1 = F_SR_NA_1.Serialize(buffer, in headerReq1, in F_SR_NA_1);
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    length1 = 0;
                                    newNos = 0;
                                }
                            }
                            else
                            {
                                headerReq1 = new AsduPacketHeader_2_2(AsduType.F_LS_NA_1, SQ.Single, 1,
                                COT.FILE_TRANSFER,
                                PN.Positive,
                                initAddr: header.InitAddr,
                                commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                var F_LS_NA_1 = new F_LS_NA_1(address, nof, nos, LSQ.FileSendWithoutDeactivation, fileCache.Cs);

                                length1 = F_LS_NA_1.Serialize(buffer, in headerReq1, in F_LS_NA_1);
                            }

                            _packetSender!.Send(buffer[..length1]);
                        }
                        break;
                    case AFQ.FileConfirmPositive:
                        _fileReadStates.Remove(nof, out _);
                        _fileProvider.CompliteReadFile(nof);
                        break;
                    default:
                        throw new Iec60870_5_104ApplicationException(header, $"address {address} nof {nof} nos {nos} afq {afq}");
                }

                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Сегмент F_SG_NA_1 = 125,
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nof"></param>
    /// <param name="nos"></param>
    /// <param name="segment"></param>
    /// <param name="ct"></param>
    internal void Process_F_SG_NA_1(AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, Span<byte> segment, CancellationToken ct)
    {
        var fileReady = _fileWriteStates.TryGetValue(nof, out var fileCache);
        if (!fileReady)
            throw new Iec60870_5_104ApplicationException(header);

        if (!fileCache!.Sections.TryGetValue(nos, out var sections))
        {
            fileCache!.Sections[nos] = sections = [];
        }
        sections.Add(segment.ToArray());
    }

    /// <summary>
    /// Директория F_DR_TA_1 = 126
    /// </summary>
    /// <param name="header"></param>
    /// <param name="address"></param>
    /// <param name="nodf"></param>
    /// <param name="lof"></param>
    /// <param name="sof"></param>
    /// <param name="dateTime"></param>
    /// <param name="timeStatus"></param>
    /// <param name="ct"></param>
    internal void Process_F_DR_TA_1(AsduPacketHeader_2_2 header, ushort address, ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus, CancellationToken ct)
    {
        throw new Iec60870_5_104ApplicationException(header, $"address {address} nodf {nodf} lof {lof} sof {sof} dateTime {dateTime} timeStatus {timeStatus}");
    }
}

