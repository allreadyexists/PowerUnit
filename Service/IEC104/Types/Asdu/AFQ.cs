namespace PowerUnit.Asdu;

[Flags]
public enum AFQ : byte
{
    Default = 0,
    FileConfirmPositive = 1,
    FileConfirmNegative = 2,
    SectionConfirmPositive = 3,
    SectionConfirmNegative = 4,

    MemoryUnavalible = 1 << 4,
    ErrorCS = 2 << 4,
    UnexpectedCommunicationService = 3 << 4,
    UndefinedFileName = 4 << 4,
    UndefinedSectionName = 5 << 4
}
