using PowerUnit.Asdu;

using System.Collections.Concurrent;

namespace PowerUnit;

public partial class Iec60870_5_104ServerApplicationLayer
{
    private sealed class FileReadState
    {
        public SortedDictionary<byte, byte> SectionControlSum { get; } = [];
        public FileInfo? FileInfo { get; set; }
    }

    private readonly ConcurrentDictionary<ushort, FileReadState> _fileReadStates = [];

    private static uint AdjustSectionLength(int fileSize, uint sectionMinLength)
    {
        var calcSectionLength = (uint)fileSize / 253;
        return calcSectionLength < sectionMinLength ? sectionMinLength : calcSectionLength;
    }

    private static uint SectionLength(int fileSize, byte sectionName, uint sectionMinLength)
    {
        var sectionLength = AdjustSectionLength(fileSize, sectionMinLength);

        var sectionOffset = (sectionName - 1) * sectionLength;
        if (sectionOffset + sectionLength < fileSize)
            return sectionLength;
        else
        {
            if (fileSize > sectionOffset)
                return (uint)(fileSize - sectionOffset);
            else
                return 0;
        }
    }

    private uint GetSectionLength(ushort fileName, byte sectionName, uint sectionMinLength)
    {
        var fileReadState = _fileReadStates.GetOrAdd(fileName, (fileName) =>
        {
            var fileReadState = new FileReadState();
            var fileInfo = _fileProvider.GetFileInfo(fileName);
            if (fileInfo != null && fileInfo.Length < 0xFFFFFF)
            {
                fileReadState.FileInfo = fileInfo;
            }

            return fileReadState;
        });

        return SectionLength((int)(fileReadState.FileInfo?.Length ?? 0), sectionName, sectionMinLength);
    }

    private FileReadState UpdateFileReadStateSectionCs(ushort fileName, byte sectionName, byte sectionCs)
    {
        return _fileReadStates.AddOrUpdate(fileName, (fileName) =>
        {
            var fileReadState = new FileReadState();
            var fileInfo = _fileProvider.GetFileInfo(fileName);
            if (fileInfo != null && fileInfo.Length < 0xFFFFFF)
            {
                fileReadState.FileInfo = fileInfo;
            }

            return fileReadState;
        },
        (fileName, oldValue) =>
        {
            if (!oldValue.SectionControlSum.TryGetValue(sectionName, out var value))
            {
                oldValue.SectionControlSum[sectionName] = sectionCs;
            }

            return oldValue;
        });
    }

    private byte GetFileCs(ushort fileName)
    {
        var fileInfo = _fileProvider?.GetFileInfo(fileName);
        var sectionLength = AdjustSectionLength((int)(fileInfo?.Length ?? 0), _applicationLayerOption.FileSectionSize);
        var sectionFullCount = fileInfo.Length / sectionLength;
        var sectionPartCount = fileInfo.Length % sectionLength != 0 ? 1 : 0;

        _fileReadStates.TryGetValue(fileName, out var fileReadState);

        byte fileCs = 0;
        for (byte sectionName = 1; sectionName <= sectionFullCount + sectionPartCount; sectionName++)
        {
            byte sectionCs;
            if (!fileReadState.SectionControlSum.TryGetValue(sectionName, out sectionCs))
            {
                var section = _fileProvider.GetSection(fileName, sectionName, SectionLength((int)fileInfo.Length, sectionName, _applicationLayerOption.FileSectionSize));
                var newSectionCs = section.Aggregate<byte, byte>(0, (x, t) => (byte)(x + t));
                fileReadState.SectionControlSum[sectionName] = sectionCs = newSectionCs;
            }

            fileCs = (byte)(fileCs + sectionCs);
        }

        return fileCs;
    }

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
                    var headerReq = new AsduPacketHeader_2_2(AsduType.F_SC_NA_1, SQ.Single, 1,
                        COT.FILE_TRANSFER,
                        PN.Positive,
                        initAddr: header.InitAddr,
                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                    var F_SC_NA_1 = new F_SC_NA_1(address, nof, 1, SCQ.FileRequest);
                    var length = F_SC_NA_1.Serialize(buffer, ref headerReq, ref F_SC_NA_1);
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
                var headerReq = new AsduPacketHeader_2_2(AsduType.F_SC_NA_1, SQ.Single, 1,
                    COT.FILE_TRANSFER,
                    PN.Positive,
                    initAddr: header.InitAddr,
                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var F_SC_NA_1 = new F_SC_NA_1(address, nof, nos, SCQ.SectionRequest);
                var length = F_SC_NA_1.Serialize(buffer, ref headerReq, ref F_SC_NA_1);
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

                                var length = F_DR_TA_1_Sequence.Serialize(buffer, ref headerReq, new Address3(address), F_DR_TA_1_Sequences[0..directoryContentChank.Length]);
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
                            var length = F_DR_TA_1_Sequence.Serialize(buffer, ref headerReq, new Address3(address), []);
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
                                var fileInfo = _fileProvider.GetFileInfo(nof);
                                var fileReady = fileInfo != null && fileInfo.Length < 0xFFFFFF;

                                var headerReq = new AsduPacketHeader_2_2(AsduType.F_FR_NA_1, SQ.Single, 1,
                                    COT.FILE_TRANSFER,
                                    PN.Positive,
                                    initAddr: header.InitAddr,
                                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                var F_FR_NA_1 = new F_FR_NA_1(address, nof, fileReady ? (uint)fileInfo!.Length : 0, fileReady ? FRQ.Positive : FRQ.Negative);

                                var length = F_FR_NA_1.Serialize(
                                    buffer, ref headerReq, ref F_FR_NA_1);
                                _packetSender!.Send(buffer[..length]);
                                break;
                            case SCQ.FileRequest: // запрос секции
                                //ответ IEC 60870 - 5 - 101 / 104 ASDU: ASDU = 1 F_SR_NA_1 File    IOA = 0 'section ready'
                                var fileInfo1 = _fileProvider.GetFileInfo(nof);
                                var fileReady1 = fileInfo1 != null && fileInfo1.Length < 0xFFFFFF;

                                var lengthOfSection = GetSectionLength(nof, nos, _applicationLayerOption.FileSectionSize);
                                var headerReq1 = new AsduPacketHeader_2_2(AsduType.F_SR_NA_1, SQ.Single, 1,
                                COT.FILE_TRANSFER,
                                PN.Positive,
                                initAddr: header.InitAddr,
                                commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                var F_SR_NA_1 = new F_SR_NA_1(address, nof, nos, fileReady1 ? lengthOfSection : 0, fileReady1 ? SRQ.Ready : SRQ.NotReady);

                                var length1 = F_SR_NA_1.Serialize(buffer, ref headerReq1, ref F_SR_NA_1);
                                _packetSender!.Send(buffer[..length1]);
                                break;
                            case SCQ.SectionRequest: // запрос секции
                                                     // ответ - готовые сегменты IEC 60870-5-101/104 ASDU: ASDU=1 F_SG_NA_1 File    IOA=0 'segment'
                                                     // когда сегменты закончатся: IEC 60870 - 5 - 101 / 104 ASDU: ASDU = 1 F_LS_NA_1 File    IOA = 0 'last section, last segment'
                                var fileInfo2 = _fileProvider.GetFileInfo(nof)!;
                                var section = _fileProvider.GetSection(nof, nos, AdjustSectionLength((int)fileInfo2.Length, _applicationLayerOption.FileSectionSize));
                                var sectionCs = section.Aggregate<byte, byte>(0, (x, t) => (byte)(x + t));
                                UpdateFileReadStateSectionCs(nof, nos, sectionCs);

                                foreach (var segment in section.Chunk((byte)_applicationLayerOption.FileSegmentSize))
                                {
                                    var headerReq2 = new AsduPacketHeader_2_2(AsduType.F_SG_NA_1, SQ.Single, 1,
                                        COT.FILE_TRANSFER,
                                        PN.Positive,
                                        initAddr: header.InitAddr,
                                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                    var F_SG_NA_1 = new F_SG_NA_1(address, nof, nos, (byte)segment.Length);

                                    var length2 = F_SG_NA_1.Serialize(buffer, ref headerReq2, ref F_SG_NA_1, segment);
                                    _packetSender!.Send(buffer[..length2]);
                                }

                                var headerReq3 = new AsduPacketHeader_2_2(AsduType.F_LS_NA_1, SQ.Single, 1,
                                        COT.FILE_TRANSFER,
                                        PN.Positive,
                                        initAddr: header.InitAddr,
                                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                                var F_LS_NA_1 = new F_LS_NA_1(address, nof, nos, LSQ.SectionSendWithoutDeactivation, sectionCs);

                                var length3 = F_LS_NA_1.Serialize(buffer, ref headerReq3, ref F_LS_NA_1);
                                _packetSender!.Send(buffer[..length3]);
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
                var headerReq = new AsduPacketHeader_2_2(AsduType.F_AF_NA_1, SQ.Single, 1,
                    COT.FILE_TRANSFER,
                    PN.Positive,
                    initAddr: header.InitAddr,
                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                var F_AF_NA_1 = new F_AF_NA_1(address, nof, nos, (lsq is LSQ.FileSendWithoutDeactivation or LSQ.FileSendWithDeactivation) ? AFQ.FileConfirmPositive : AFQ.SectionConfirmPositive);
                var length = F_AF_NA_1.Serialize(buffer, ref headerReq, ref F_AF_NA_1);
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
                        var newNos = (byte)(nos + 1);
                        var fileInfo1 = _fileProvider.GetFileInfo(nof);
                        var fileReady1 = fileInfo1 != null && fileInfo1!.Length < 0xFFFFFF;
                        var lengthOfSection = GetSectionLength(nof, newNos, _applicationLayerOption.FileSectionSize);

                        AsduPacketHeader_2_2 headerReq1;
                        int length1;

                        if (lengthOfSection > 0)
                        {
                            headerReq1 = new AsduPacketHeader_2_2(AsduType.F_SR_NA_1, SQ.Single, 1,
                            COT.FILE_TRANSFER,
                            PN.Positive,
                            initAddr: header.InitAddr,
                            commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                            var F_SR_NA_1 = new F_SR_NA_1(address, nof, newNos, fileReady1 ? lengthOfSection : 0, fileReady1 ? SRQ.Ready : SRQ.NotReady);

                            length1 = F_SR_NA_1.Serialize(buffer, ref headerReq1, ref F_SR_NA_1);
                        }
                        else
                        {
                            headerReq1 = new AsduPacketHeader_2_2(AsduType.F_LS_NA_1, SQ.Single, 1,
                            COT.FILE_TRANSFER,
                            PN.Positive,
                            initAddr: header.InitAddr,
                            commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);

                            var F_LS_NA_1 = new F_LS_NA_1(address, nof, nos, LSQ.FileSendWithoutDeactivation, GetFileCs(nof));

                            length1 = F_LS_NA_1.Serialize(buffer, ref headerReq1, ref F_LS_NA_1);
                        }

                        _packetSender!.Send(buffer[..length1]);
                        break;
                    case AFQ.FileConfirmPositive:
                        _fileProvider.CloseFile(nof);
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
    internal void Process_F_SG_NA_1(AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, byte[] segment, CancellationToken ct)
    {
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

