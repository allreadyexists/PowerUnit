namespace PowerUnit.Service.IEC104.Types.Asdu;

[Flags]
public enum SCQ : byte
{
    Default = 0,
    FileSelect = 1,
    FileRequest = 2,
    FileDeactivate = 3,
    FileDelete = 4,
    SectionSelect = 5,
    SectionRequest = 6,
    SectionDeactivate = 7,

    MemoryUnavalible = 1 << 4,
    ErrorCS = 2 << 4,
    UnexpectedCommunicationService = 3 << 4,
    UndefinedFileName = 4 << 4,
    UndefinedSectionName = 5 << 4
}
