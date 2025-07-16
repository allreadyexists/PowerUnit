using PowerUnit.Service.IEC104.Types.Asdu;

namespace PowerUnit.Service.IEC104.Types;

public interface IASDUNotification : IDisposable
{
    void Notify_M_SP(in ASDUPacketHeader_2_2 header, ushort address, SIQ_Value value, SIQ_Status diq, DateTime dateTime, TimeStatus status);
    void Notify_M_DP(in ASDUPacketHeader_2_2 header, ushort address, DIQ_Value value, DIQ_Status diq, DateTime dateTime, TimeStatus status);
    void Notify_M_ME(in ASDUPacketHeader_2_2 header, ushort address, float value, QDS_Status diq, DateTime dateTime, TimeStatus status);
    void Notify_C_IC_NA(in ASDUPacketHeader_2_2 header, ushort address, QOI qoi);

    void Notify_C_TS_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort fbp);
    void Notify_C_TS_TA(in ASDUPacketHeader_2_2 header, ushort address, ushort tsc, DateTime dateTime, TimeStatus status);

    void Notify_C_RD_NA(in ASDUPacketHeader_2_2 header, ushort address);

    void Notify_M_EI_NA(in ASDUPacketHeader_2_2 header, ushort address, COI coi);

    void Notify_C_CS_NA(in ASDUPacketHeader_2_2 header, ushort address, DateTime dateTime, TimeStatus timeStatus);

    void Notify_C_CI_NA(in ASDUPacketHeader_2_2 header, ushort address, QCC qcc);

    void Notify_F_FR_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, uint lof, FRQ frq);
    void Notify_F_SR_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, uint los, SRQ frq);
    void Notify_F_SC_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, SCQ scq);
    void Notify_F_LS_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, LSQ lsq, byte chs);
    void Notify_F_AF_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, AFQ afq);
    void Notify_F_SG_NA(in ASDUPacketHeader_2_2 header, ushort address, ushort nof, byte nos, Span<byte> segment);
    void Notify_F_DR_TA(in ASDUPacketHeader_2_2 header, ushort address, ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus);

    void Notify_Unknown_Asdu_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw);
    void Notify_Unknown_Cot_Raw(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw);
    void Notify_Unknown_Exception(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw, Exception ex);

    bool Notify_CommonAsduAddress(in ASDUPacketHeader_2_2 header, Span<byte> asduInfoRaw);
}

