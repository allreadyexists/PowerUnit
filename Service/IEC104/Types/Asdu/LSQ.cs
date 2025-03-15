namespace PowerUnit.Service.IEC104.Types.Asdu;

public enum LSQ : byte
{
    FileSendWithoutDeactivation = 1,
    FileSendWithDeactivation = 2,
    SectionSendWithoutDeactivation = 3,
    SectionSendWithDeactivation = 4
}
