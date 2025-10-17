using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Application;

public sealed class IEC60870_5_104ClientApplicationLayer : IASDUNotification
{
    public void Dispose() { }
    public bool Notify_CommonAsduAddress(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
        return true;
    }
    public void Notify_C_CI_NA(in ASDUPacketHeader_2_2 header, ushort address, QCC qcc) { }
    public void Notify_C_CS_NA(in ASDUPacketHeader_2_2 header, ushort address, DateTime dateTime, TimeStatus timeStatus) { }
    public void Notify_C_IC_NA(in ASDUPacketHeader_2_2 header, ushort address, QOI qoi) { }
    public void Notify_C_RD_NA(in ASDUPacketHeader_2_2 header, ushort address) { }
    public void Notify_C_TS_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort fbp) { }
    public void Notify_C_TS_TA(in ASDUPacketHeader_2_2 header, ushort address, ushort tsc, DateTime dateTime, TimeStatus status) { }
    public void Notify_F_AF_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, AFQ afq) { }
    public void Notify_F_DR_TA(in ASDUPacketHeader_2_2 header, ushort address, ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus) { }
    public void Notify_F_FR_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, uint lof, FRQ frq) { }
    public void Notify_F_LS_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, LSQ lsq, byte chs) { }
    public void Notify_F_SC_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, SCQ scq) { }
    public void Notify_F_SG_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, Span<byte> segment) { }
    public void Notify_F_SR_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, uint los, SRQ frq) { }
    public void Notify_M_DP(in ASDUPacketHeader_2_2 header, ushort address, DIQ_Value value, DIQ_Status diq, DateTime dateTime, TimeStatus status) { }
    public void Notify_M_EI_NA(in ASDUPacketHeader_2_2 header, ushort address, COI coi)
    {
    }
    public void Notify_M_ME(in ASDUPacketHeader_2_2 header, ushort address, float value, QDS_Status diq, DateTime dateTime, TimeStatus status)
    {
    }
    public void Notify_M_SP(in ASDUPacketHeader_2_2 header, ushort address, SIQ_Value value, SIQ_Status diq, DateTime dateTime, TimeStatus status) { }
    public void Notify_Unknown_Asdu_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
    }
    public void Notify_Unknown_Cot_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw)
    {
    }
    public void Notify_Unknown_Exception(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, Exception ex)
    {
    }
}

