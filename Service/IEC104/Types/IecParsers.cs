using PowerUnit.Asdu;

using System.Runtime.InteropServices;

namespace PowerUnit;

public interface IApciNotification
{
    void NotifyI(ref PacketI packetI);
    void NotifyS(ref PacketS packetS);
    void NotifyU(ref PacketU packetU);
}

public interface IAsduNotification : IDisposable
{
    void Notify_M_SP(ref AsduPacketHeader_2_2 header, ushort address, SIQ_Value value, SIQ_Status diq, DateTime dateTime, TimeStatus status);
    void Notify_M_DP(ref AsduPacketHeader_2_2 header, ushort address, DIQ_Value value, DIQ_Status diq, DateTime dateTime, TimeStatus status);
    void Notify_M_ME(ref AsduPacketHeader_2_2 header, ushort address, float value, QDS_Status diq, DateTime dateTime, TimeStatus status);
    void Notify_C_IC_NA(ref AsduPacketHeader_2_2 header, ushort address, QOI qoi);

    void Notify_C_TS_NA(ref AsduPacketHeader_2_2 header, ushort address, ushort fbp);
    void Notify_C_TS_TA(ref AsduPacketHeader_2_2 header, ushort address, ushort tsc, DateTime dateTime, TimeStatus status);

    void Notify_C_RD_NA(ref AsduPacketHeader_2_2 header, ushort address);

    void Notify_M_EI_NA(ref AsduPacketHeader_2_2 header, ushort address, COI coi);

    void Notify_C_CS_NA(ref AsduPacketHeader_2_2 header, ushort address, DateTime dateTime, TimeStatus timeStatus);

    void Notify_C_CI_NA(ref AsduPacketHeader_2_2 header, ushort address, QCC qcc);

    void Notify_F_FR_NA(ref AsduPacketHeader_2_2 header, ushort address, ushort nof, uint lof, FRQ frq);
    void Notify_F_SR_NA(ref AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, uint los, SRQ frq);
    void Notify_F_SC_NA(ref AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, SCQ scq);
    void Notify_F_LS_NA(ref AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, LSQ lsq, byte chs);
    void Notify_F_AF_NA(ref AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, AFQ afq);
    void Notify_F_SG_NA(ref AsduPacketHeader_2_2 header, ushort address, ushort nof, byte nos, byte[] segment);
    void Notify_F_DR_TA(ref AsduPacketHeader_2_2 header, ushort address, ushort nodf, uint lof, SOF sof, DateTime dateTime, TimeStatus timeStatus);

    void Notify_Unknown_Asdu_Raw(ref AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw);
    void Notify_Unknown_Cot_Raw(ref AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw);
    void Notify_Unknown_Exception(ref AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw, Exception ex);

    bool Notify_CommonAsduAddress(ref AsduPacketHeader_2_2 header, Span<byte> asduInfoRaw);
}

public static class IecParsers
{
    public static void Parse(this IApciNotification apciNotification, Span<byte> buffer)
    {
        var apci = MemoryMarshal.AsRef<APCI>(buffer[..APCI.Size]);
        if (apci.TryGetIPacket(out var iPacket))
            apciNotification.NotifyI(ref iPacket);
        else if (apci.TryGetUPacket(out var uPacket))
            apciNotification.NotifyU(ref uPacket);
        else if (apci.TryGetSPacket(out var sPacket))
            apciNotification.NotifyS(ref sPacket);
        else
            throw new ArgumentException(nameof(apci));
    }

    public static void ParseHeader(Span<byte> headerBuffer, out AsduPacketHeader_2_2 header)
    {
        header = MemoryMarshal.AsRef<AsduPacketHeader_2_2>(headerBuffer);
    }
}

